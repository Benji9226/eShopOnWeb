from fastapi import FastAPI
from ..services.catalog_service import CatalogService
from ..messaging.catalog_messaging import RabbitMQClient
from ..models.catalog import Catalog

app = FastAPI(title="Order Service")

order_service = CatalogService()
mq_client = RabbitMQClient()

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/order")
def create_order(catalog: Catalog):
    processed = order_service.process_order(catalog)
    mq_client.publish("orders", catalog)
    return {"status": "received", "order": processed}