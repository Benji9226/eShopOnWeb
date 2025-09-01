from ..models.catalog import Catalog

class CatalogService:
    def process_catalog(self, catalog: Catalog) -> dict:
        return {
            "id": catalog.id,
            "product": catalog.product,
            "quantity": catalog.quantity,
            "processed": True
        }