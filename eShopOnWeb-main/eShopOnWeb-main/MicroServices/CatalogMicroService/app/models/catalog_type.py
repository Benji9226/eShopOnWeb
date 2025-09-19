from sqlmodel import Field, Relationship, SQLModel
from typing import List, TYPE_CHECKING
from app.models.base_entity import BaseEntity
from sqlalchemy import Column, String

if TYPE_CHECKING:
    from app.models.catalog_item import CatalogItem 

class CatalogType(BaseEntity, table=True):
    type: str = Field(sa_column=Column(String(100), nullable=False))

    items: List["CatalogItem"] = Relationship(back_populates="catalog_type")
