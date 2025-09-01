from sqlmodel import Relationship
from typing import List, Optional, TYPE_CHECKING
from app.models.base_entity import BaseEntity

if TYPE_CHECKING:
    from app.models.catalog_item import CatalogItem 

class CatalogBrand(BaseEntity, table=True):
    brand: str

    items: List["CatalogItem"] = Relationship(back_populates="catalog_brand")