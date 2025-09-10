from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from app import models, schemas, events
from decimal import Decimal
import asyncio

async def calculate_total(items: list[schemas.OrderItemCreate]) -> Decimal:
    return sum((Decimal(i.unitprice) * i.units) for i in items)

async def create_order(db: AsyncSession, order_in: schemas.OrderCreate):
    order = models.Order(
        buyer_id=order_in.buyer_id,
        shiptoaddress_street=order_in.shiptoaddress_street,
        shiptoaddress_city=order_in.shiptoaddress_city,
        shiptoaddress_state=order_in.shiptoaddress_state,
        shiptoaddress_country=order_in.shiptoaddress_country,
        shiptoaddress_zipcode=order_in.shiptoaddress_zipcode,
    )
    for it in order_in.items:
        order.items.append(
            models.OrderItem(
                itemordered_catalogitemid=it.itemordered_catalogitemid,
                itemordered_productname=it.itemordered_productname,
                itemordered_pictureuri=it.itemordered_pictureuri,
                unitprice=it.unitprice,
                units=it.units,
            )
        )

    db.add(order)
    await db.commit()
    await db.refresh(order)

    payload = {
        "type": "OrderCreated",
        "version": 1,
        "data": {
            "order_id": order.id,
            "buyer_id": order.buyer_id,
            "items": [
                {
                    "catalog_item_id": it.itemordered_catalogitemid,
                    "product_name": it.itemordered_productname,
                    "unit_price": str(it.unitprice),
                    "units": it.units,
                }
                for it in order.items
            ],
            "shipping": {
                "street": order.shiptoaddress_street,
                "city": order.shiptoaddress_city,
                "state": order.shiptoaddress_state,
                "country": order.shiptoaddress_country,
                "zip": order.shiptoaddress_zipcode,
            },
        },
    }
    asyncio.create_task(events.publish_event("order.created", payload))
    return order

async def get_order(db: AsyncSession, order_id: int):
    q = await db.execute(select(models.Order).where(models.Order.id == order_id))
    return q.scalars().first()

async def list_orders_for_buyer(db: AsyncSession, buyer_id: str):
    q = await db.execute(
        select(models.Order)
        .where(models.Order.buyer_id == buyer_id)
        .order_by(models.Order.order_date.desc())
    )
    return q.scalars().all()
