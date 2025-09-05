from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from app.database import get_db
from app.repositories.catalog_item_repository import CatalogItemRepository
from app.models.catalog_item import CatalogItem

router = APIRouter(prefix="/items", tags=["catalog-items"])


@router.get("/")
async def read_items(db: AsyncSession = Depends(get_db)):
    repo = CatalogItemRepository(db)
    return await repo.list_all()


@router.post("/")
async def add_item(
    name: str,
    description: str,
    price: float,
    picture_uri: str,
    catalog_type_id: int,
    catalog_brand_id: int,
    db: AsyncSession = Depends(get_db),
):
    repo = CatalogItemRepository(db)
    item = CatalogItem(
        name=name,
        description=description,
        price=price,
        picture_uri=picture_uri,
        catalog_type_id=catalog_type_id,
        catalog_brand_id=catalog_brand_id,
    )
    return await repo.add(item)