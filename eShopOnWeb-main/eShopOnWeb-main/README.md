# eShopOnWeb Project – Running Locally with Docker

This project contains the main web application and multiple microservices, including Catalog and Order services. You can run everything in Docker using a single command.

---

## 1. Create the Docker Network

Before running the project, create the shared Docker network used by all services:

docker network create eshop-on-web-net

All microservices and the main app will connect to this network to communicate with each other.

---

## 2. Build and Start the Containers

From the root folder of the project (eShopOnWeb-main), use Docker Compose to build and launch all services:

docker compose -f all-services.yml up --build

This will:

- Build the necessary images.
- Start the containers for the main web app, Order microservice, and Catalog microservice.
- Spin up required databases, RabbitMQ, and PGAdmin containers.
- Connect all services to the shared eshop-on-web-net Docker network.

---

## 3. Access the Application

Once the containers are running, open your browser and go to:

- Web application: http://localhost:5106
- Catalog API: http://localhost:8000
- Order API: http://localhost:8001
- PGAdmin (Catalog): http://localhost:8080
- PGAdmin (Order): http://localhost:8081

Make sure the ports are available and not blocked by other applications.

---

## 4. Stop the Containers

To stop all running services, press Ctrl+C in the terminal or run:

docker compose -f all-services.yml down

This will shut down and remove all containers.

---

## 5. Remove the Docker Network (Optional)

If you want to clean up the Docker network after stopping the containers:

docker network rm eshop-on-web-net
