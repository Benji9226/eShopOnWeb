from sqlalchemy import select
from typing import Optional
from app.models.catalog_item import CatalogItem

def catalog_filter_specification(brand_id: Optional[int] = None, type_id: Optional[int] = None):
    stmt = select(CatalogItem)
    if brand_id is not None:
        stmt = stmt.where(CatalogItem.catalog_brand_id == brand_id)
    if type_id is not None:
        stmt = stmt.where(CatalogItem.catalog_type_id == type_id)
    return stmt

def catalog_filter_paginated_specification(skip: int = 0, take: Optional[int] = None,
                                           brand_id: Optional[int] = None,
                                           type_id: Optional[int] = None):
    if take in (0, None):
        take = 1_000_000  # equivalent to int.MaxValue
    stmt = catalog_filter_specification(brand_id, type_id).offset(skip).limit(take)
    return stmt
