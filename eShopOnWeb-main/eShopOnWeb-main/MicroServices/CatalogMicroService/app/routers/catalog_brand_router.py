from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from app.database import get_db
from app.repositories.catalog_brand_repository import CatalogBrandRepository
from app.models.catalog_brand import CatalogBrand

router = APIRouter(prefix="/brands", tags=["catalog-brands"])


@router.get("/")
async def read_brands(db: AsyncSession = Depends(get_db)):
    repo = CatalogBrandRepository(db)
    return await repo.list_all()


@router.post("/")
async def add_brand(brand: str, db: AsyncSession = Depends(get_db)):
    repo = CatalogBrandRepository(db)
    catalog_brand = CatalogBrand(brand=brand)
    return await repo.add(catalog_brand)