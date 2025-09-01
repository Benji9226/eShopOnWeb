from fastapi import FastAPI
from ..services.catalog_service import CatalogService
from ..messaging.catalog_messaging import RabbitMQClient
from ..models.catalog import Catalog

app = FastAPI(title="Catalog Service")

catalog_service = CatalogService()
mq_client = RabbitMQClient()

@app.get("/health")
def health():
    return {"status": "ok"}

"""
@app.post("/catalog")
def create_catalog(catalog: Catalog):
    processed = catalog_service.process_catalog(catalog)
    mq_client.publish("catalogs", catalog)
    return {"status": "received", "catalog": processed}
"""