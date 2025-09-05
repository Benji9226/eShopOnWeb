from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from app.database import get_db
from app.repositories.catalog_type_repository import CatalogTypeRepository
from app.models.catalog_type import CatalogType

router = APIRouter(prefix="/types", tags=["catalog-types"])


@router.get("/")
async def read_types(db: AsyncSession = Depends(get_db)):
    repo = CatalogTypeRepository(db)
    return await repo.list_all()


@router.post("/")
async def add_type(type_name: str, db: AsyncSession = Depends(get_db)):
    repo = CatalogTypeRepository(db)
    catalog_type = CatalogType(type=type_name)
    return await repo.add(catalog_type)