from sqlmodel import Relationship
from typing import List, TYPE_CHECKING
from app.models.base_entity import BaseEntity

if TYPE_CHECKING:
    from app.models.catalog_item import CatalogItem 

class CatalogType(BaseEntity, table=True):
    type: str

    items: List["CatalogItem"] = Relationship(back_populates="catalog_type")