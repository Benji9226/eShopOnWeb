# Eksamensspørgsmål - eShopOnWeb Projekt

## Spørgsmål 2: Sikkerhed i Microservices og Systemintegration

### 2a. Almindelige sikkerhedsudfordringer i Microservice-arkitektur

#### 1. Autentifikation og Autorisering mellem Services

**Udfordring:** Når services kommunikerer, skal hver service verificere identiteten af den kaldende service.

**Løsning i projektet – JWT og mTLS:**
- **JWT (JSON Web Tokens):** API Gateway validerer JWT-tokens fra klienter
- **mTLS (Mutual TLS):** Gateway bruger klientcertifikater når det kalder downstream services

Eksempel fra `ApiGateway.go`:
```go
// Gateway validerer JWT fra Authorization header
func validateJWT(tokenString string) (*jwt.Claims, error) {
    token, err := jwt.ParseWithClaims(tokenString, &jwt.Claims{}, ...)
    // Hvis token er invalid → 401 Unauthorized
}
```

Eksempel fra `.NET Web` (`src/Infrastructure/Authentication/TokenService.cs`):
```csharp
// Web service genererer JWT ved opstart
var token = tokenService.GenerateToken();
return new BearerTokenHandler(token);
```

**Fordele:**
- JWT er stateless; ingen session DB nødvendig
- mTLS sikrer at kun autoriserede services kan kommunikere
- Certificater roteres automatisk ved deployment

---

#### 2. Secret Management

**Udfordring:** Private keys, databaseadgangskoder, og JWT secrets skal ikke være i versionskontrol.

**Løsning – Docker Secrets Volume:**
- `gen-secrets.sh` genererer secrets en gang ved opstart
- Secrets gemmes i Docker volume `/secrets` (aldrig committed til git)
- Services monterer volumet read-only

```bash
# Fra gen-secrets.sh
if [[ ! -f "$SECRETS/jwt/secret.key" ]]; then
  openssl rand -base64 32 > "$SECRETS/jwt/secret.key"
fi

# Per-service server certs (names må matche service navne)
for svc in orders catalog; do
  gen_server "$svc"
done
```

Services læser fra filen:
```csharp
var secretKeyPath = "/secrets/jwt/secret.key";
if (!File.Exists(secretKeyPath))
{
    throw new Exception($"Secret key not found at: {secretKeyPath}");
}
string secretKey = File.ReadAllText(secretKeyPath).Trim();
services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<TokenService>>();
    return new TokenService(secretKey, logger);
});
```

**Fordele:**
- Secrets roteres uden code changes
- Ingen hardcoding af passwords
- Read-only mount sikrer at secrets ikke modificeres af containers

---

#### 3. Service-to-Service Communication Security

**Udfordring:** Services skal kun acceptere requests fra autoriserede kilder.

**Løsning – Client Certificates (mTLS):**
```bash
# gen-secrets.sh genererer client cert for Gateway
gen_client gateway  
# Output: /secrets/clients/gateway/client.{crt,key}
```

Gateway bruger certifikatet når det kalder andre services:
```go
// ApiGateway.go - mTLS client configuration
tlsConfig := &tls.Config{
    Certificates: []tls.Certificate{clientCert},
    ClientCAs:    caPool,
    VerifyPeerCertificate: verifyCert,
}
```

Docker Compose konfiguration:
```yaml
catalog:
  environment:
    TLS_CERT: /secrets/services/catalog/server.crt
    TLS_KEY: /secrets/services/catalog/server.key
    TLS_CA: /secrets/ca/ca.crt
  volumes:
    - dev-secrets:/secrets:ro
```

**Fordele:**
- Bilateral autentifikation (både klient og server verificeres)
- Certificater udløber og tvinger rotation
- Kan implementere certificate pinning i gateway

---

#### 4. Data i Transit (Encryption)

**Udfordring:** Data mellem services skal være krypteret.

**Løsning:** HTTPS/TLS på alle interne forbindelser
- Catalog Service: `https://catalog:8000`
- Order Service: `https://orders:8001`
- Basket Service: `https://basket:8000`

**Network Segmentation:**
```yaml
networks:
  eshop-on-web-net:
    external: true
```

Alle services kører på samme Docker network, som er isoleret fra host.

**Fordele:**
- Kryptering af alle data i transit
- Certificater valideres ved forbindelse
- Diffie-Hellman key exchange sikrer forward secrecy

---

#### 5. API Rate Limiting & DDoS Protection

**Udfordring:** En service kan blive overbelastet af mange requests.

**Løsning i ApiGateway:**
Gateway kan implementere rate limiting før requests når downstream services:
```go
// Pseudo-code - kan implementeres
type RateLimiter struct {
    maxRequestsPerSecond int
    bucket map[clientIP]requestCount
}

func (rl *RateLimiter) checkLimit(clientIP string) bool {
    if rl.bucket[clientIP] > rl.maxRequestsPerSecond {
        return false  // Reject request
    }
}
```

---

#### 6. Logging & Audit Trail

**Udfordring:** Skal kunne spore hvad der skete og hvem der gjorde det.

**Løsning – Centraliseret Logging (Loki):**
```yaml
# Monitoring/docker-compose-grafana.yml
loki:
  image: grafana/loki
  ports:
    - "3100:3100"
    
promtail:
  image: grafana/promtail
  # Shipper container logs til Loki

grafana:
  ports:
    - "3001:3001"
```

Eksempel på struktureret logging:
```csharp
logger.LogInformation(
    "API request | Method={Method} | Path={Path} | ClientIP={ClientIP} | StatusCode={StatusCode}",
    request.Method, request.Path, request.RemoteIP, response.StatusCode
);
```

Operatører kan query logs i Grafana:
```
{job="docker", container_name="web"} | "API request"
```

---

### 2b. Sikkerhed i Messaging-systemer (RabbitMQ)

#### 1. Message Tampering – Beskytte mod manipulation

**Udfordring:** En ondsindet aktør kunne manipulere beskeder i RabbitMQ.

**Løsning – Message Signing/Validation:**

I eShopOnWeb valideres messages gennem OrderService og Web-listeners:

```csharp
// Fra Infrastructure/RabbitMQ/Services/RabbitMqService.cs
public async Task PublishOrderCreatedEvent(Order order)
{
    var orderEvent = new OrderCreatedEvent
    {
        OrderId = order.Id,
        UserId = order.UserId,
        Items = order.OrderItems,
        Timestamp = DateTime.UtcNow
    };
    
    // Message sendes til RabbitMQ exchange
    channel.BasicPublish(
        exchange: "orders.topic",
        routingKey: "order.created",
        body: JsonSerializer.SerializeToUtf8Bytes(orderEvent)
    );
}
```

**Best Practice – Message Signing:**
```csharp
var messageJson = JsonSerializer.Serialize(orderEvent);
using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
{
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(messageJson));
    var signature = Convert.ToBase64String(hash);
    
    // Send både message + signature
    var signedMessage = new { message = orderEvent, signature = signature };
    // Publish...
}

// Ved modtagelse: verify signature
var computedSignature = ComputeHmacSignature(messageJson, secretKey);
if (computedSignature != receivedSignature)
{
    throw new SecurityException("Message tampered!");
}
```

**Fordele:**
- Entydig verifikation af message integritet
- Detekterer hvis hacker modificerer ordrebeløb, bruger ID, etc.
- Kan kombineres med encryption for fuld sikkerhed

---

#### 2. Unauthorized Message Access – Adgangskontrol til queues

**Udfordring:** Enhver service skal ikke kunne læse fra alle queues.

**Løsning – RabbitMQ-brugerrettigheder:**

```yaml
# docker-compose-adminPages.yml (dev)
rabbitmq:
  image: rabbitmq:3-management
  ports:
    - "5672:5672"      # AMQP port (uenkrypteret i dev)
    - "15672:15672"    # Management UI
  environment:
    RABBITMQ_DEFAULT_USER: guest
    RABBITMQ_DEFAULT_PASS: guest
```

**I produktion:**
```bash
# RabbitMQ CLI commands
rabbitmqctl add_user orders_service <secure_password>
rabbitmqctl add_user storage_service <secure_password>

# Tildel permissions per service
# orders_service kan kun læse/skrive til orders-queues
rabbitmqctl set_permissions -p / orders_service "^orders\." "^orders\." "^orders\."

# storage_service kan kun læse/skrive til stock-queues
rabbitmqctl set_permissions -p / storage_service "^stock\." "^stock\." "^stock\."

# Deaktiver default guest user
rabbitmqctl delete_user guest
```

**Fordele:**
- Least privilege principle: hver service får kun nødvendige permissions
- Hvis en service bliver compromised, attacker kan ikke tilgå andre queues
- Audit logs viser hvilken user sendte hver message

---

#### 3. Message Delivery Assurance – Håndtere lost/duplicate messages

**Udfordring:** RabbitMQ garanterer ikke message delivery; netværksfejl kan miste eller duplikere messages.

**Løsning – Dead Letter Exchange (DLX) og Retry Logic:**

```csharp
// Infrastructure/RabbitMQ/Services/RabbitMqService.cs
public async Task SubscribeToOrderEvents()
{
    // Definer queue med DLX
    var queueArgs = new Dictionary<string, object>
    {
        { "x-dead-letter-exchange", "orders.dlx" },
        { "x-message-ttl", 3600000 },  // 1 time TTL
        { "x-max-length", 1000000 }    // Max 1M messages
    };
    
    channel.QueueDeclare(
        queue: "orders.events",
        durable: true,                 // Queue overlever broker restart
        exclusive: false,
        autoDelete: false,
        arguments: queueArgs
    );
    
    // Consumer med acknowledgment
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += async (model, ea) =>
    {
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            await ProcessOrderEvent(message);
            
            // Kun ack hvis processing var succesfuld
            channel.BasicAck(ea.DeliveryTag, false);
            logger.LogInformation("Order event processed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to process order: {ex.Message}");
            
            // Requeue message så det prøves igen
            channel.BasicNack(ea.DeliveryTag, false, true);
        }
    };
    
    channel.BasicConsume(queue: "orders.events", autoAck: false, consumer: consumer);
}
```

**Dead Letter Queue (DLQ) håndtering:**
- Messages der failer 3 gange sendes til `orders.dlx`
- Operator inspicerer via RabbitMQ management UI
- Kan reprocesses manuelt eller logs for debugging

**Fordele:**
- Messages går ikke tabt ved fejl
- Automatisk retry uden manual intervention
- DLQ giver visibility til problematiske messages

---

#### 4. Encryption af Messages i Transit

**Udfordring:** RabbitMQ messages kan aflæses hvis ikke krypteret.

**Løsning – TLS for RabbitMQ (AMQPS):**

```yaml
# docker-compose-adminPages.yml (produktion)
rabbitmq:
  image: rabbitmq:3-management
  environment:
    RABBITMQ_SSL_CERTFILE: /secrets/services/rabbitmq/server.crt
    RABBITMQ_SSL_KEYFILE: /secrets/services/rabbitmq/server.key
    RABBITMQ_SSL_CACERTFILE: /secrets/ca/ca.crt
  ports:
    - "5671:5671"    # AMQPS (encrypted)
    - "15671:15671"  # Management HTTPS
  volumes:
    - dev-secrets:/secrets:ro
```

Services forbinder med TLS:
```csharp
var factory = new ConnectionFactory()
{
    HostName = "rabbitmq",
    Port = 5671,
    Ssl = new SslOption
    {
        Enabled = true,
        ServerName = "rabbitmq",
        CertPath = "/secrets/ca/ca.crt",
        Certs = new[] { clientCert }
    }
};
var connection = factory.CreateConnection();
```

**Fordele:**
- AES-256 encryption af alle messages i transit
- Certificater valideres (MITM-beskyttelse)
- Samme certificate infrastructure som services bruger

---

#### 5. Monitoring & Audit Trail for Messages

**Udfordring:** Skal kunne spore hvilke messages der blev sendt/modtaget og af hvem.

**Løsning – Struktureret Logging:**

```csharp
logger.LogInformation(
    "RabbitMQ Event | Exchange={Exchange} | RoutingKey={RoutingKey} | Message={Message} | Timestamp={Timestamp}",
    "orders.topic", "order.created", JsonSerializer.Serialize(orderEvent), DateTime.UtcNow
);
```

**Grafana Loki query:**
```
{job="docker", container_name="web"} 
| "RabbitMQ Event" 
| RoutingKey="order.created"
```

**Fordele:**
- Audit trail af alle events
- Kan trace message flow gennem systemet
- Detekterer anomal activity (f.eks. uventet høj message rate)

---

#### 6. Poison Message Handling

**Udfordring:** Hvis en message altid forårsager exception, bliver den ved med at blive retried infinit.

**Løsning – Max Retry Count:**

```csharp
var queueArgs = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "orders.dlx" },
    { "x-dead-letter-routing-key", "order.failed" },
    { "x-max-length-bytes", 1000000000 }  // 1GB max
};

// Hvis message requeued > 3 gange, send til DLX
int retryCount = 0;
consumer.Received += async (model, ea) =>
{
    try
    {
        await ProcessOrderEvent(message);
        channel.BasicAck(ea.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        retryCount++;
        if (retryCount >= 3)
        {
            // Send til dead letter queue
            channel.BasicNack(ea.DeliveryTag, false, false);
            logger.LogError($"Message poisoned after 3 retries: {message}");
        }
        else
        {
            channel.BasicNack(ea.DeliveryTag, false, true);  // Requeue
        }
    }
};
```

**Fordele:**
- Forhindrer infinite retry loops
- Operator kan inspicere poison messages
- Systemet stabiliseres selvom der er forkerte messages

---

## Samlet Sikkerhedsmodel

| Lag | Trussel | Løsning | Implementation |
|-----|---------|---------|-----------------|
| **Authentication** | Uautoriseret adgang | JWT + mTLS | ApiGateway validerer tokens |
| **Authorization** | Uautoriseret service kald | Certificates | gen-secrets.sh mTLS certs |
| **Secrets** | Kompromitterede credentials | Secret rotation | Docker volume + env vars |
| **Data in transit** | Eavesdropping | TLS/HTTPS | HTTPS på alle endpoints |
| **RabbitMQ** | Message tampering | HMAC signing | OrderCreatedEvent signeres |
| **RabbitMQ** | Unauthorized access | User permissions | rabbitmqctl set_permissions |
| **RabbitMQ** | Lost messages | DLX + retry | BasicAck/Nack implementering |
| **RabbitMQ** | Eavesdropping | AMQPS | TLS for RabbitMQ |
| **Logging** | No audit trail | Centraliseret logging | Loki + Grafana |
| **Rate limiting** | DoS attacks | Request throttling | ApiGateway kan implementere |

---

## Konklusion

eShopOnWeb implementerer defense-in-depth strategi:
1. **Autentifikation** på gateway-niveau (JWT)
2. **Service-to-service sikkerhed** via mTLS
3. **Secret management** uden versionskontrol
4. **Message integritet** ved signing
5. **Message durability** via DLX
6. **Centraliseret observabilitet** for security incidents

Denne kombination sikrer at systemet kan modstå både accidentelle fejl og bevidste angreb på microservice-niveau.
