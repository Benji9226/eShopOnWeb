from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from app.models.catalog_item import CatalogItem
from app.specifications.catalog_specifications import catalog_filter_specification, catalog_filter_paginated_specification
from sqlalchemy import select, func



class CatalogItemRepository:
    def __init__(self, db: AsyncSession):
        self.db = db

    async def get_by_id(self, id: int) -> CatalogItem | None:
        return await self.db.get(CatalogItem, id)

    async def list_paged(self, skip: int, take: int, brand_id: int | None = None, type_id: int | None = None):
        stmt = catalog_filter_paginated_specification(skip=skip, take=take, brand_id=brand_id, type_id=type_id)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def count(self, brand_id: int | None = None, type_id: int | None = None):
        stmt = catalog_filter_specification(brand_id=brand_id, type_id=type_id)
        count_stmt = select(func.count()).select_from(stmt.subquery())
        result = await self.db.execute(count_stmt)
        return result.scalar_one()

    async def add(self, item: CatalogItem) -> CatalogItem:
        self.db.add(item)
        await self.db.commit()
        await self.db.refresh(item)
        return item

    async def update(self, item: CatalogItem) -> CatalogItem:
        await self.db.commit()
        await self.db.refresh(item)
        return item

    async def delete(self, item: CatalogItem):
        await self.db.delete(item)
        await self.db.commit()