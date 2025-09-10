from .value_objects import CatalogItemOrdered
from pydantic import BaseModel

class OrderItem(BaseModel):
    item_ordered: CatalogItemOrdered
    unit_price: float
    units: int
