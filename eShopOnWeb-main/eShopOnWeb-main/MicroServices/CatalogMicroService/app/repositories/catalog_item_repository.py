from sqlalchemy.ext.asyncio import AsyncSession
from sqlmodel import select
from app.models.catalog_item import CatalogItem


class CatalogItemRepository:
    def __init__(self, session: AsyncSession):
        self.session = session

    async def get_by_id(self, item_id: int) -> CatalogItem | None:
        result = await self.session.execute(
            select(CatalogItem).where(CatalogItem.id == item_id)
        )
        return result.scalar_one_or_none()

    async def list_all(self) -> list[CatalogItem]:
        result = await self.session.execute(select(CatalogItem))
        return result.scalars().all()

    async def add(self, catalog_item: CatalogItem) -> CatalogItem:
        self.session.add(catalog_item)
        await self.session.commit()
        await self.session.refresh(catalog_item)
        return catalog_item

    async def delete(self, item_id: int) -> None:
        catalog_item = await self.get_by_id(item_id)
        if catalog_item:
            await self.session.delete(catalog_item)
            await self.session.commit()

    async def update(self, item_id: int, updated_item: CatalogItem) -> CatalogItem | None:
        catalog_item = await self.get_by_id(item_id)
        if catalog_item:
            catalog_item.name = updated_item.name
            catalog_item.description = updated_item.description
            catalog_item.price = updated_item.price
            
            await self.session.commit()
            await self.session.refresh(catalog_item)
        return None