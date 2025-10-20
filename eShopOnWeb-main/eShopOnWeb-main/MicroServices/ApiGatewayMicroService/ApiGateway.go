package main

import (
	"io"
	"log"
	"net/http"
	"strings"
)

type BackendService struct {
	Name      string
	Instances []string
	Counter   int
}

// Round-robin load balancer
func (s *BackendService) GetNextInstance() string {
	if len(s.Instances) == 0 {
		return ""
	}
	instance := s.Instances[s.Counter%len(s.Instances)]
	s.Counter++
	return instance
}

// Forward the request to the appropriate microservice
func ForwardRequest(w http.ResponseWriter, r *http.Request, service *BackendService) {
	backendURL := service.GetNextInstance()
	if backendURL == "" {
		http.Error(w, "no backend available", http.StatusServiceUnavailable)
		return
	}

	// Normalize slashes: remove trailing slash from backendURL if needed
	if strings.HasSuffix(backendURL, "/") && strings.HasPrefix(r.URL.Path, "/") {
		backendURL = backendURL[:len(backendURL)-1]
	}

	// Log the full forwarded URL for debugging
	log.Printf("Forwarding %s %s -> %s%s", r.Method, r.URL.Path, backendURL, r.URL.Path)

	// Build the new request
	proxyReq, err := http.NewRequest(r.Method, backendURL+r.URL.Path, r.Body)
	if err != nil {
		http.Error(w, "failed to create request", http.StatusInternalServerError)
		return
	}

	// Copy headers
	proxyReq.Header = r.Header

	// Send request
	client := &http.Client{}
	resp, err := client.Do(proxyReq)
	if err != nil {
		http.Error(w, "backend service unavailable", http.StatusServiceUnavailable)
		return
	}
	defer resp.Body.Close()

	// Copy response headers
	for k, v := range resp.Header {
		w.Header()[k] = v
	}

	w.WriteHeader(resp.StatusCode)
	io.Copy(w, resp.Body)
}

func main() {
	// Define microservices with **base URLs only** (no trailing slashes)
	services := map[string]*BackendService{
		"catalog": {Name: "catalog", Instances: []string{"http://catalog-api:8000"}},
		"orders":  {Name: "orders", Instances: []string{"http://order:8001"}},
	}

	// Generic handler to strip service prefix and forward requests
	handler := func(serviceName string) http.HandlerFunc {
		return func(w http.ResponseWriter, r *http.Request) {
			service, ok := services[serviceName]
			if !ok {
				http.Error(w, "service not found", http.StatusNotFound)
				return
			}

			// Trim the service prefix (e.g., /catalog -> /items)
			r.URL.Path = strings.TrimPrefix(r.URL.Path, "/"+serviceName)

			ForwardRequest(w, r, service)
		}
	}

	// Routes for each service
	http.HandleFunc("/catalog/", handler("catalog"))
	http.HandleFunc("/orders/", handler("orders"))

	log.Println("API Gateway running on :8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
