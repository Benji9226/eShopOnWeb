from sqlmodel import select
from app.models import CatalogBrand, CatalogType, CatalogItem
from sqlalchemy.ext.asyncio import AsyncSession

async def seed_catalog_brands(session: AsyncSession):
    brands_to_add = [
        CatalogBrand(brand="Azure"),
        CatalogBrand(brand=".NET"),
        CatalogBrand(brand="Visual Studio"),
        CatalogBrand(brand="SQL Server"),
        CatalogBrand(brand="Other")
    ]
    
    for brand in brands_to_add:
        result = await session.execute(select(CatalogBrand).where(CatalogBrand.brand == brand.brand))
        if not result.scalars().first():  
            session.add(brand)
    
    await session.commit()

async def seed_catalog_types(session: AsyncSession):
    types_to_add = [
        CatalogType(type="Mug"),
        CatalogType(type="T-Shirt"),
        CatalogType(type="Sheet"),
        CatalogType(type="USB Memory Stick")
    ]
    
    for catalog_type in types_to_add:
        result = await session.execute(select(CatalogType).where(CatalogType.type == catalog_type.type))
        if not result.scalars().first(): 
            session.add(catalog_type)
    
    await session.commit()


async def seed_catalog_items(session: AsyncSession):
    items_to_add = [
        CatalogItem(catalog_brand_id=2, catalog_type_id=2, name=".NET Bot Black Sweatshirt", description=".NET Bot Black Sweatshirt", price=19.5, picture_uri="http://catalogbaseurltobereplaced/images/products/1.png"),
        CatalogItem(catalog_brand_id=1, catalog_type_id=2, name=".NET Black & White Mug", description=".NET Black & White Mug", price=8.50, picture_uri="http://catalogbaseurltobereplaced/images/products/2.png"),
    ]
    
    for item in items_to_add:
        result = await session.execute(select(CatalogItem).where(CatalogItem.name == item.name))
        if not result.scalars().first():
            session.add(item)
    
    await session.commit()


async def seed_db(session: AsyncSession):
    await seed_catalog_brands(session)
    await seed_catalog_types(session)
    await seed_catalog_items(session)
