# Sikkerhed i Microservices - eShopOnWeb

> **10-15 min fremlæggelse**

---

## 📊 Agenda

1. **Sikkerhedsudfordringer** i microservice-arkitektur
2. **Løsninger** - Hvordan eShopOnWeb håndterer det
3. **Messaging-sikkerhed** med RabbitMQ
4. **Defense-in-depth** modellen

---

## 🔴 Slide 1: De store udfordringer

### Hvorfor er microservices sværere at sikre end monolitter?

```
Monolith:                    Microservices:
┌─────────────┐             ┌──────┐  ┌──────┐  ┌──────┐
│ En app      │             │Svc A │→ │Svc B │→ │Svc C │
│ En database │             └──────┘  └──────┘  └──────┘
└─────────────┘                ↓        ↓        ↓
                            Postgres  Postgres  RabbitMQ
```

**Nye trusler:**
- ❌ Service-to-service autentifikation
- ❌ Hemmeligheder fordelt på N services
- ❌ Kommunikation over netværk (ikke i-memory)
- ❌ Event-baseret messaging kan blive aflæst/manipuleret

---

## 🔐 Slide 2: Autentifikation mellem Services

### Problem: Service A kalder Service B. Hvordan ved B at det er virkelig A?

I en monolith er det lettere - alt køres i samme proces. Men når services køres separat, skal der være en mekanisme til at verificere identiteten.

**Løsning i eShopOnWeb: JWT + mTLS**

#### JWT (For API kald fra eksterne klienter)

JWT står for JSON Web Token. Det er som et pas, der indeholder brugerens identitet og rettigheder. Gateway genererer et JWT-token når brugeren logger ind, og alle requests sendes derefter med dette token.

**Sådan virker det:**
1. Bruger logger ind via Web-applikationen
2. TokenService genererer et JWT-token med brugerens ID og permissions
3. Alle requests fra browseren indeholder: `Authorization: Bearer <token>`
4. Gateway verificerer tokenet før den kalder downstream services
5. Hvis token er ugyldigt eller udløbet → 401 Unauthorized

**Fordele ved JWT:**
- Stateless: Gateway behøver ikke at slå tokens op i en database
- Self-contained: Alle oplysninger er i token'et selv
- Kan expire: Token udløber efter fx 1 time, automatisk rotation

#### mTLS (For Service-to-Service kommunikation)

mTLS (Mutual TLS) er stærkere end JWT. Det betyder at BÅDE gateway og downstream service verificerer hinanden med digitale certifikater.

**Sådan virker det:**
1. `gen-secrets.sh` genererer TLS-certifikater for hver service
2. Gateway har et klientcertifikat (bevis på at det er gateway)
3. Catalog Service har et servercertifikat (bevis på at det er catalog)
4. Når gateway kalder Catalog, udbytter de certifikater
5. Begge parter verificerer at certifikatet er signeret af samme trusted CA

**Fordele ved mTLS:**
- Bilateral autentifikation (begge parter verificeres)
- Certificater kan ikke forfalske uden adgang til private keys
- Automatisk refresh når certifikater roteres

✅ **Resultat:** En hacker kan ikke logge ind som gateway-service uden det rigtige certifikat

---

## 🔑 Slide 3: Secret Management

### Problem: Passwords og private keys skal være sikre

En meget almindelig fejl er at hardcode hemmeligheder i koden. Hvis nogen får adgang til git-repoen (eller en container image), kan de læse alle passwords.

**Løsning: Docker Secrets Volume**

eShopOnWeb bruger en smart tilgang:
1. `gen-secrets.sh` køres automatisk når containeren starter (første gang)
2. Scriptet genererer alle hemmeligheder og gemmer dem i `/secrets` folderen
3. Hemmeligheder monteres som read-only volumes i containers
4. Services læser fra filer i stedet for miljøvariabler

**Hemmeligheder der genereres:**
- `jwt/secret.key` - Brugt til at signere JWT-tokens
- `ca/ca.crt` og `ca/ca.key` - Root certificate authority
- `services/catalog/server.crt` - Catalog servicens TLS certifikat
- `clients/gateway/client.crt` - Gateway's klientcertifikat

**Vigtige sikkerhedspunkter:**
- ✅ Hemmeligheder genereres dynamisk, ikke committed til git
- ✅ Read-only mount betyder services kan ikke skrive/modificere secrets
- ✅ Docker volume betyder hemmeligheder kun eksisterer i memory under runtime
- ✅ Når container stopper, forsvinder hemmeligheder (ny generation næste gang)

**Real-world eksempel:**
Hvis en udvikler ved en fejltagelse committer kode med password, er det ikke katastrofalt fordi det øjeblik container restartes, får det et nyt password fra secrets volumet.

✅ **Resultat:** Secrets håndteres som infrastruktur (operatørens ansvar), ikke som kode (udviklers ansvar)

---

## 🔒 Slide 4: Kryptering i Transit

### Problem: Data sendes mellem services over netværk

Når data sendes over netværk (også hvis det er bare over Docker netværket), kan en attacker potentielt aflytter trafikken. Data kunne indeholde brugerinformation, ordredetaljer, eller andre sensitive data.

**Løsning: HTTPS/TLS overalt**

eShopOnWeb bruger TLS (Transport Layer Security) til ALL kommunikation mellem services. Det betyder at selv hvis nogen aflytter netværket, kan de kun se krypteret garbage.

**Sådan virker det:**
1. Alle service-to-service URLs bruger `https://` i stedet for `http://`
2. TLS certifikater (fra gen-secrets.sh) bruges til at etablere krypteret forbindelser
3. Data krypteres med AES-256 (meget stærk standard)
4. Certifikater verificeres for at forhindre man-in-the-middle attacks

**Konkrete eksempler i systemet:**
- Web app til Gateway: `https://gateway:8090` (krypteret)
- Gateway til Catalog: `https://catalog:8000` (mTLS + krypteret)
- Gateway til Order: `https://orders:8001` (mTLS + krypteret)

**Sikkerhedsniveauer:**
- Web → Gateway: TLS + JWT (to lag)
- Gateway → Services: TLS + mTLS (tre lag - encryption + certifikat verificering)

✅ **Resultat:** Selv hvis en hacker kommer på Docker-netværket, kan de ikke læse ordredata fordi alt er krypteret

---

## 📝 Slide 5: Logging & Audit Trail

### Problem: Hvordan sporer vi sikkerhedsbrud?

Hvis systemet udsættes for et angreb, skal man kunne se hvad der skete. Uden logs kan man ikke bevise hvem der gjorde hvad eller når det skete.

**Løsning: Centraliseret logging (Loki + Grafana)**

I stedet for at skulle logge ind på hver container for at se logs, sender alle containers deres logs til en central Loki database. Grafana gør det muligt at søge og visualisere logs.

**Arkitektur:**
1. Hver container skriver til stdout/stderr
2. Promtail (Docker agent) indsamler logs
3. Logs sendes til Loki (log database)
4. Grafana queries Loki og viser grafer/statistikker

**Praktiske eksempler på hvad der logges:**
- `API request | Method=POST | Path=/orders | StatusCode=200` - Succesfuld ordre
- `API request | Method=GET | Path=/catalog | StatusCode=401` - Unauthorized attempt (mulig attack)
- `RabbitMQ Event | OrderId=123 | UserId=456 | Timestamp=2025-12-03T10:00:00Z` - Event tracking

**Hvordan operatører bruger det:**
```
Grafana søg: {container_name="web"} | status="401"
Resultat: Alle failed auth attempts mod web-servicen
→ Viser hvorfra requests kom fra (IP adresse)
→ Kan blokere mistænkelig IP
```

**Compliance fordele:**
- Audit trail: Kan bevise at følge GDPR, PCI-DSS regler
- Forensics: Hvis der sker en sikkerhedsbrud, kan man gå tilbage og se hvad der skete
- Alerting: Kan sætte alarms når der sker mistænkelig aktivitet (fx 100+ failed logins)

✅ **Resultat:** Fuldstændig visibility i systemet for at detecte og respondera på sikkerhedshændelser

---

## 💬 Slide 6: RabbitMQ Sikkerhed - Problemet

### Problem: Messages kan aflæses eller manipuleres

RabbitMQ er en message broker - services sender ordrer, betalinger og events gennem den. Hvis messages ikke er sikre, kan en attacker:
1. **Aflæse messages** - Se hvad brugere køber, deres kreditkortdata, etc.
2. **Manipulere messages** - Ændre ordrebeløb fra 1000 DKK til 1 DKK
3. **Duplikere messages** - Køre samme ordre flere gange
4. **Slette messages** - Så ordren aldrig bliver behandlet

**Konkret eksempel fra eShopOnWeb:**
Når brugeren placerer en ordre, sender Order Service en `OrderCreatedEvent` message til RabbitMQ. Web Service lytter på denne message og opdaterer brugerens ordrehistorie. Hvis messages ikke er sikre:
```
Hacker: "Jeg aflytter RabbitMQ"
         ↓
         Ser: OrderId=123, UserId=456, Items=[Product1, Product2], Amount=1000 DKK
         Manipulerer: Amount=1 DKK
         Sender tilbage til RabbitMQ
         ↓
         Web Service modtager ændret ordre (uden at vide det)
         ↓
         Bruger får ordre til 1 DKK (fraud!)
```

### Løsninger: 3 lag sikkerhed

---

## 🛡️ Slide 7: RabbitMQ - Lag 1: Message Signing

### Beskytte mod tampering

**Idé:** Hvis vi signerer hver message, kan modtageren detektere hvis den blev manipuleret.

**Sådan virker det:**
1. Order Service har et hemmeligt nøgle (fx "my-super-secret-key")
2. Før Order Service sender `OrderCreatedEvent` til RabbitMQ:
   - Den konverterer order'en til JSON: `{"OrderId":123,"UserId":456,"Amount":1000}`
   - Den beregner en HMAC-signature af JSON'en med den hemmelige nøgle
   - Signature er som et fingerprint - hvis JSON ændres, bliver signature anderledes
3. Order Service sender både JSON og signature til RabbitMQ
4. Web Service modtager message:
   - Den beregner selv HMAC-signature af den modtagne JSON
   - Den sammenligner: Hvis `modtagen_signature == beregnet_signature` → OK
   - Hvis de ikke matcher → Nogen har manipuleret message!

**Sikkerhedseffekt:**
```
Hacker prøver at ændre: OrderId=123 → OrderId=999
         ↓
         Han ved ikke den hemmelige nøgle
         ↓
         Han beregner forkert signature
         ↓
         Web Service detekterer mismatch
         ↓
         Message kasseres, sikkerhedsalarm
```

**Real-world analogi:** Det er som en forseglet brev. Hvis nogen åbner det og ændrer indholdet, er segling brækket og modtageren ser at det blev manipuleret.

✅ **Resultat:** Detekterer hvis hacker ændrer ordrebeløb, bruger ID eller andet

---

## 👥 Slide 8: RabbitMQ - Lag 2: Access Control

### Sikre at services kun læser deres egne queues

**Problem:** Default RabbitMQ bruger (`guest`/`guest`) kan læse ALLE queues. Hvis Order Service bliver kompromitteret, har hackeren adgang til alle messages i alle queues - inkl. Stock Service's private messages.

**Løsning: Per-service users med begrænsede permissions**

**Development (for simpel lokal test):**
- Default guest bruger - alle har adgang til alt
- OK fordi det er lokalt og kun til udvikling

**Production (rigtig sikkerhed):**
1. Opret separate users for hver service:
   - `orders_service` bruger
   - `storage_service` bruger
   - `web_service` bruger
2. Giv hver bruger KUN permission til deres egne queues:
   - `orders_service` kan KUN læse/skrive til queues der starter med `orders.*`
   - `storage_service` kan KUN læse/skrive til queues der starter med `stock.*`
   - `web_service` kan KUN læse `orders.events` (events fra order service)

**Sikkerhedseffekt:**
```
Scenario: Order Service server får compromised af hacker
         ↓
         Hacker får Order Service's credentials
         ↓
         Hacker kan læse `orders.*` queues - men IKKE `stock.*`
         ↓
         Hacker kan ikke se eller manipulere Stock Service's data
         ↓
         Damage containeret! (Principle of Least Privilege)
```

**Real-world analogi:** Det er som at give forskellige medarbejdere forskellige nøgler. Receptionist har nøgle til reception, men ikke til vault. Hvis receptionist bliver kidnappet, kan de ikke få adgang til penge.

✅ **Resultat:** Hvis én service bliver hacket, kan attacker ikke tilgå andre services' data

---

## 🔄 Slide 9: RabbitMQ - Lag 3: Encryption (TLS)

### Sikre at messages ikke aflæses

**Problem:** Selvom vi har signet messages og sat access control, kan trafikken til RabbitMQ stadig aflytters. AMQP (RabbitMQ's protokol) sender data uenkrypteret som standard.

**Løsning: AMQPS (AMQP + TLS)**

AMQPS er samme som AMQP, men med TLS encryption oven på. Det betyder:
1. Forbindelsen til RabbitMQ bliver krypteret (AES-256)
2. Messages kan ikke aflæses selvom nogen sniffer netværket
3. TLS certifikater bruges til at verificere at det er den rigtige RabbitMQ broker

**Development vs Production:**
- **Development** (localhost): AMQP port 5672 OK (ikke real data)
- **Production**: AMQPS port 5671 (encrypted)

**Sådan bruges det:**
1. RabbitMQ får et TLS certifikat (fra gen-secrets.sh)
2. Services konfigureres til at forbinde via port 5671 i stedet for 5672
3. Services verificerer RabbitMQ's certifikat før de sender data

**Sikkerhedseffekt:**
```
Scenario: Hacker sidder på samme netværk og sniffer trafik
         ↓
         AMQP (port 5672): Hacker ser OrderId=123, Amount=1000 - DATA LÆST!
         AMQPS (port 5671): Hacker ser krypteret noise - USELESS!
```

**Kombineret sikkerhed (Lag 1-3 sammen):**
```
Message journey:
1. Order Service signerer message (Lag 1)
2. Order Service binder med orders_service bruger (Lag 2)
3. Order Service sender over AMQPS encrypted (Lag 3)
         ↓
         RabbitMQ modtager
         ↓
         Web Service modtager over AMQPS encrypted (Lag 3)
         ↓
         Web Service verificerer bruger (Lag 2)
         ↓
         Web Service verificerer signature (Lag 1)
         ↓
         Message er sikker på 3 niveauer!
```

✅ **Resultat:** Selvom hacker aflytter netværket, kan de ikke læse eller manipulere data

---

## ⚡ Slide 10: Dead Letter Queues - Håndter fejl

### Problem: Hvis en message altid failer, bliver den retry'et infinit

**Scenario:** Order Service sender `OrderCreatedEvent`. Web Service prøver at behandle det, men servicen er nede. Message requeues. Omtrent 10 minutter senere Web Service starter igen, men den samme message failer igen fordi der er en bug i koden. Uden Dead Letter Queue ville denne message blive retry'et infinit, og systemet ville blive overbelastet.

**Løsning: Dead Letter Exchange (DLX)**

DLX er som et "affalds-queue". Når en message fejler flere gange, sendes den automatisk til DLX i stedet for at blive retry'et infinit.

**Sådan virker det:**
1. Messages får en TTL (Time To Live) - fx 1 time
2. Hvis message fejler, requeues den automatisk
3. Hvis den fejler igen efter X antal gange (fx 3), sendes den til DLX
4. Operator kan inspiciere messages i DLX:
   - Debugge hvorfor de fejler
   - Fikse rootcause
   - Reprocesse messages når den er fikset

**Praktisk eksempel fra eShopOnWeb:**
```
Scenario: En ordre har OrderId=null (bug i Order Service)
Web Service prøver: ParseOrder(order) → FEJL: OrderId kan ikke være null
         ↓
         Message requeues (retry #1)
         ↓
         Web Service prøver igen → FEJL igen (bug ikke fikset)
         ↓
         Message requeues (retry #2)
         ↓
         Samme fejl igen (retry #3)
         ↓
         Maximum retries nået → Message sendes til orders.dlx queue
         ↓
         Operator ser i RabbitMQ dashboard: "1 message i orders.dlx"
         ↓
         Operator debugger, finder bug i Order Service
         ↓
         Operator fikser bug og deployer ny version
         ↓
         Operator reprocesser message fra DLX manually
         ↓
         Web Service behandler succesfuldt!
```

**Vigtige fordele:**
- ✅ Forhindrer infinite retry loops (systemet korsfester)
- ✅ Prevents starvation (andre messages kan behandles)
- ✅ Gives visibility (operator ved at der er problemer)
- ✅ Enables recovery (kan genprocesses når bug er fikset)

✅ **Resultat:** Systemet stabiliseres selvom der er bug - poison messages går ikke tabt

---

## 🏗️ Slide 11: Defense-in-Depth Model

### Alle sikkerhedslagene sammen

| Layer | Trussel | Løsning | Eksempel |
|-------|---------|---------|---------|
| **Auth** | Unauthorized API access | JWT validation | Gateway checker bearer token |
| **Service-to-Service** | Service spoofing | mTLS certificates | Catalog kræver client cert fra gateway |
| **Secrets** | Hardcoded credentials | Volume mounts | `/secrets/jwt/secret.key` |
| **Data Transit** | Eavesdropping | HTTPS/TLS | `https://catalog:8000` |
| **Messages** | Tampering | HMAC signing | OrderEvent får signature |
| **Queue Access** | Unauthorized reads | User permissions | `rabbitmqctl set_permissions` |
| **Message Transit** | Interception | AMQPS (TLS) | RabbitMQ port 5671 |
| **Durability** | Lost messages | Dead Letter Queue | DLX efter 3 retries |
| **Audit** | No compliance trail | Loki logging | Grafana queries |

---

## 💡 Slide 12: Key Takeaways

### Hvad skal I huske?

✅ **1. Defense-in-depth** - Lag på lag af sikkerhed  
✅ **2. Automate secrets** - Genereres, roteres, aldrig håndkoded  
✅ **3. Mutual TLS** - Services verificerer hinanden  
✅ **4. Message integrity** - Signing forhindrer tampering  
✅ **5. Monitoring** - Loki + Grafana for incident detection  
✅ **6. Graceful failures** - DLX for poison messages  

**eShopOnWeb's sikkerhed er ikke magisk - det er arkitektur!**

---

## 📚 Slide 13: Vigtige filer

### Hvor er det hele implementeret?

| Fil | Formål |
|-----|--------|
| `gen-secrets.sh` | TLS cert + JWT secret generation |
| `MicroServices/ApiGatewayMicroService/ApiGateway.go` | JWT validation, mTLS client config |
| `src/Infrastructure/Authentication/TokenService.cs` | JWT generation og bearer handler |
| `src/Infrastructure/RabbitMQ/RabbitMqService.cs` | Event publishing + DLX setup |
| `docker-compose-adminPages.yml` | RabbitMQ configuration |
| `Monitoring/docker-compose-grafana.yml` | Loki + Grafana logging |

**Live demo:**
```powershell
# Start system
docker network create eshop-on-web-net
docker compose -f all-services.yml up --build

# Besøg Grafana
http://localhost:3001 (admin/admin)
```

---

## Q&A

### Spørgsmål?
