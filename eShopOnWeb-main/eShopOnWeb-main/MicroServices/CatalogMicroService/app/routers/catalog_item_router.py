from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from app.database import get_db
from app.repositories.catalog_item_repository import CatalogItemRepository
from app.dto.catalog_item_dto import CatalogItemDTO, ListPagedCatalogItemResponse
from typing import Optional

router = APIRouter(prefix="/catalog-items", tags=["catalog-items"])
base_url="http://localhost:8000"  # adjust for your domain

# GET /catalog-items/{id}
@router.get("/{catalog_item_id}", response_model=CatalogItemDTO)
async def get_catalog_item(catalog_item_id: int, db: AsyncSession = Depends(get_db)):
    repo = CatalogItemRepository(db)
    item = await repo.get_by_id(catalog_item_id)
    if not item:
        raise HTTPException(status_code=404, detail="Catalog item not found")
    dto = CatalogItemDTO.model_validate(item)
    return dto

# GET /catalog-items?pageSize=&pageIndex=&catalogBrandId=&catalogTypeId=
@router.get("/", response_model=ListPagedCatalogItemResponse)
async def list_catalog_items(
    pageSize: int = 10,
    pageIndex: int = 0,
    catalogBrandId: Optional[int] = None,
    catalogTypeId: Optional[int] = None,
    db: AsyncSession = Depends(get_db)
    ):
    
    repo = CatalogItemRepository(db)

    # count total items
    total_items = await repo.count(catalogBrandId, catalogTypeId)

    # fetch paginated items
    items = await repo.list_paged(
        skip=pageIndex * pageSize,
        take=pageSize,
        brand_id=catalogBrandId,
        type_id=catalogTypeId
    )

    # map to DTOs and apply UriComposer
    catalog_items = []
    for i in items:
        dto = CatalogItemDTO.model_validate(i)
        catalog_items.append(dto)

    # compute page count like C# code
    page_count = (total_items + pageSize - 1) // pageSize if pageSize else (1 if total_items else 0)

    response = ListPagedCatalogItemResponse(
        catalog_items=catalog_items,
        page_count=page_count
    )

    return response

# POST /catalog-items
@router.post("/", response_model=CatalogItemDTO)
async def create_catalog_item(item: CatalogItemDTO, db: AsyncSession = Depends(get_db)):
    repo = CatalogItemRepository(db)
    catalog_item = item.to_model()
    new_item = await repo.add(catalog_item)
    dto = CatalogItemDTO.model_validate(new_item)
    return dto

# PUT /catalog-items
@router.put("/", response_model=CatalogItemDTO)
async def update_catalog_item(item: CatalogItemDTO, db: AsyncSession = Depends(get_db)):
    repo = CatalogItemRepository(db)
    existing = await repo.get_by_id(item.id)
    if not existing:
        raise HTTPException(status_code=404, detail="Catalog item not found")
    existing.name = item.name
    existing.description = item.description
    existing.price = item.price
    existing.catalog_brand_id = item.catalog_brand_id
    existing.catalog_type_id = item.catalog_type_id
    updated_item = await repo.update(existing)
    dto = CatalogItemDTO.model_validate(updated_item)
    return dto

# DELETE /catalog-items/{id}
@router.delete("/{catalog_item_id}")
async def delete_catalog_item(catalog_item_id: int, db: AsyncSession = Depends(get_db)):
    repo = CatalogItemRepository(db)
    existing = await repo.get_by_id(catalog_item_id)
    if not existing:
        raise HTTPException(status_code=404, detail="Catalog item not found")
    await repo.delete(existing)
    return {"detail": "Deleted"}