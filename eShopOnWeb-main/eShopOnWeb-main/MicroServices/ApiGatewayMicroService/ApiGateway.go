// main.go
package main

import (
	"io"
	"log"
	"net"
	"net/http"
	"net/http/httputil"
	"net/textproto"
	"net/url"
	"os"
	"strings"
	"sync"
	"time"
)

// --------- Service & Load Balancing ---------

type BackendService struct {
	Name      string
	Instances []*url.URL
	mu        sync.Mutex
	counter   int
}

func (s *BackendService) Next() *url.URL {
	s.mu.Lock()
	defer s.mu.Unlock()
	if len(s.Instances) == 0 {
		return nil
	}
	u := s.Instances[s.counter%len(s.Instances)]
	s.counter++
	return u
}

// --------- Utilities ---------

func mustParse(raw string) *url.URL {
	u, err := url.Parse(strings.TrimRight(strings.TrimSpace(raw), "/"))
	if err != nil {
		log.Fatalf("invalid backend URL %q: %v", raw, err)
	}
	return u
}

func parseBackendList(envVal string, defaults ...string) []*url.URL {
	var parts []string
	if v := strings.TrimSpace(envVal); v != "" {
		parts = strings.Split(v, ",")
	} else {
		parts = defaults
	}
	var urls []*url.URL
	for _, p := range parts {
		if s := strings.TrimSpace(p); s != "" {
			urls = append(urls, mustParse(s))
		}
	}
	return urls
}

// join URL paths without double slashes
func joinPaths(a, b string) string {
	as := strings.HasSuffix(a, "/")
	bp := strings.HasPrefix(b, "/")
	switch {
	case as && bp:
		return a + b[1:]
	case !as && !bp:
		return a + "/" + b
	default:
		return a + b
	}
}

func dropHopByHop(h http.Header) {
	hh := []string{"Connection", "Proxy-Connection", "Keep-Alive", "Proxy-Authenticate",
		"Proxy-Authorization", "TE", "Trailers", "Transfer-Encoding", "Upgrade"}
	// Also remove per-connection headers listed in Connection
	for _, f := range h["Connection"] {
		for _, sf := range strings.Split(f, ",") {
			h.Del(textproto.CanonicalMIMEHeaderKey(strings.TrimSpace(sf)))
		}
	}
	for _, k := range hh {
		h.Del(k)
	}
}

func defaultTransport() http.RoundTripper {
	return &http.Transport{
		Proxy: http.ProxyFromEnvironment,
		DialContext: (&net.Dialer{
			Timeout:   5 * time.Second,
			KeepAlive: 30 * time.Second,
		}).DialContext,
		MaxIdleConns:          200,
		IdleConnTimeout:       90 * time.Second,
		TLSHandshakeTimeout:   5 * time.Second,
		ExpectContinueTimeout: 1 * time.Second,
	}
}

// --------- Reverse Proxy Builder ---------

func singleHostReverseProxy(target *url.URL) *httputil.ReverseProxy {
	director := func(req *http.Request) {
		req.URL.Scheme = target.Scheme
		req.URL.Host = target.Host

		req.Header.Set("X-Forwarded-Host", req.Host)
		if req.Header.Get("X-Forwarded-Proto") == "" {
			req.Header.Set("X-Forwarded-Proto", "http")
		}
		req.Host = target.Host
	}
	return &httputil.ReverseProxy{
		Director:  director,
		Transport: defaultTransport(),
		ModifyResponse: func(resp *http.Response) error {
			dropHopByHop(resp.Header)
			return nil
		},
		ErrorHandler: func(w http.ResponseWriter, r *http.Request, err error) {
			log.Printf("proxy error: %v", err)
			http.Error(w, "backend service unavailable", http.StatusBadGateway)
		},
		BufferPool: nil, // default
	}
}

// --------- Gateway Logic ---------

func forwardToService(w http.ResponseWriter, r *http.Request, svc *BackendService, strippedPath string) {
	target := svc.Next()
	if target == nil {
		http.Error(w, "no backend available", http.StatusServiceUnavailable)
		return
	}

	// Build final request path/query
	outPath := joinPaths(target.Path, strippedPath)
	outQuery := r.URL.RawQuery

	log.Printf("[%s] %s %s -> %s%s%s",
		svc.Name,
		r.Method,
		r.URL.RequestURI(),
		target.Scheme+"://"+target.Host,
		outPath,
		func() string {
			if outQuery != "" {
				return "?" + outQuery
			}
			return ""
		}(),
	)

	// Prepare clone for proxy (to avoid mutating the original)
	r2 := r.Clone(r.Context())
	r2.URL.Path = outPath
	r2.URL.RawPath = "" // let Go encode
	r2.URL.RawQuery = outQuery

	// Clean inbound hop-by-hop headers
	dropHopByHop(r2.Header)
	if r2.Body == nil {
		r2.Body = http.NoBody
	}

	// Use a per-target proxy instance
	proxy := singleHostReverseProxy(target)
	proxy.ServeHTTP(w, r2)
}

func stripFirstSegment(path, segment string) string {
	if !strings.HasPrefix(path, "/") {
		path = "/" + path
	}
	// Remove exactly the first "/segment"
	stripped := strings.TrimPrefix(path, "/"+segment)
	if stripped == "" {
		return "/"
	}
	// Ensure leading slash
	if !strings.HasPrefix(stripped, "/") {
		stripped = "/" + stripped
	}
	return stripped
}

// --------- Main ---------

func main() {

	catalogBackends := parseBackendList(
		os.Getenv("CATALOG_BACKENDS"),
		"http://catalog-api:8000",
	)
	ordersBackends := parseBackendList(
		os.Getenv("ORDERS_BACKENDS"),
		"http://orders-api:8001",
	)

	services := map[string]*BackendService{
		"catalog": {Name: "catalog", Instances: catalogBackends},
		"orders":  {Name: "orders", Instances: ordersBackends},
	}

	// Factory for path-based services
	handler := func(serviceName string) http.HandlerFunc {
		return func(w http.ResponseWriter, r *http.Request) {
			svc := services[serviceName]
			if svc == nil {
				http.Error(w, "service not found", http.StatusNotFound)
				return
			}
			stripped := stripFirstSegment(r.URL.Path, serviceName)
			forwardToService(w, r, svc, stripped)
		}
	}

	// Routes (path-prefix routing)
	http.HandleFunc("/catalog/", handler("catalog"))
	http.HandleFunc("/orders/", handler("orders"))

	// (Optional) root landing + health
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		io.WriteString(w, "gateway: try /catalog/... or /orders/...\n")
	})
	http.HandleFunc("/healthz", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		w.Header().Set("Content-Type", "text/plain")
		io.WriteString(w, "ok")
	})

	addr := ":8080"
	log.Printf("API Gateway listening on %s", addr)
	log.Fatal(http.ListenAndServe(addr, nil))
}
