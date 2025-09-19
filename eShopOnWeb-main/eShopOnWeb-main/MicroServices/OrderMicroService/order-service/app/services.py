from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from app import models, schemas, events
from decimal import Decimal
import datetime
import asyncio
from typing import List

# -----------------------
# Helper: calculate total
# -----------------------
def calculate_total(items: List[models.OrderItem]) -> float:
    return float(sum(item.unitprice * item.units for item in items))

# -----------------------
# Create a new order
# -----------------------
async def create_order(db: AsyncSession, order_in: schemas.OrderCreate) -> schemas.OrderRead:
    # Create Order model
    order = models.Order(
        buyer_id=order_in.buyer_id,
        order_date=datetime.datetime.utcnow(),
        shiptoaddress_street=order_in.shipping.street,
        shiptoaddress_city=order_in.shipping.city,
        shiptoaddress_state=order_in.shipping.state,
        shiptoaddress_country=order_in.shipping.country,
        shiptoaddress_zipcode=order_in.shipping.zip,
    )

    # Add items
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

    # Compute total
    total_amount = calculate_total(order.items)

    # Publish event (optional)
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
                    "unit_price": float(it.unitprice),
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
            "total": float(total_amount),
        },
    }
    asyncio.create_task(events.publish_event("order.created", payload))

    # Return OrderRead
    return schemas.OrderRead(
        id=order.id,
        buyer_id=order.buyer_id,
        order_date=order.order_date,
        shipping=schemas.Shipping(
            street=order.shiptoaddress_street,
            city=order.shiptoaddress_city,
            state=order.shiptoaddress_state,
            country=order.shiptoaddress_country,
            zip=order.shiptoaddress_zipcode,
        ),
        status=order.status,
        items=[
            schemas.OrderItemRead(
                id=i.id,
                itemordered_catalogitemid=i.itemordered_catalogitemid,
                itemordered_productname=i.itemordered_productname,
                itemordered_pictureuri=i.itemordered_pictureuri,
                unitprice=float(i.unitprice),
                units=i.units,
            )
            for i in order.items
        ],
        total=float(total_amount)
    )

# -----------------------
# Get single order by ID
# -----------------------
async def get_order(db: AsyncSession, order_id: int) -> schemas.OrderRead | None:
    result = await db.execute(select(models.Order).where(models.Order.id == order_id))
    order = result.scalars().first()
    if not order:
        return None

    total_amount = calculate_total(order.items)

    return schemas.OrderRead(
        id=order.id,
        buyer_id=order.buyer_id,
        order_date=order.order_date,
        shipping=schemas.Shipping(
            street=order.shiptoaddress_street,
            city=order.shiptoaddress_city,
            state=order.shiptoaddress_state,
            country=order.shiptoaddress_country,
            zip=order.shiptoaddress_zipcode,
        ),
        status=order.status,
        items=[
            schemas.OrderItemRead(
                id=i.id,
                itemordered_catalogitemid=i.itemordered_catalogitemid,
                itemordered_productname=i.itemordered_productname,
                itemordered_pictureuri=i.itemordered_pictureuri,
                unitprice=float(i.unitprice),
                units=i.units,
            )
            for i in order.items
        ],
        total=float(total_amount)
    )

# -----------------------
# List all orders for a buyer
# -----------------------
async def list_orders_for_buyer(db: AsyncSession, buyer_id: str) -> list[schemas.OrderRead]:
    result = await db.execute(
        select(models.Order)
        .where(models.Order.buyer_id == buyer_id)
        .order_by(models.Order.order_date.desc())
    )
    orders = result.scalars().all()

    return [
        schemas.OrderRead(
            id=o.id,
            buyer_id=o.buyer_id,
            order_date=o.order_date,
            shipping=schemas.Shipping(
                street=o.shiptoaddress_street,
                city=o.shiptoaddress_city,
                state=o.shiptoaddress_state,
                country=o.shiptoaddress_country,
                zip=o.shiptoaddress_zipcode,
            ),
            status=o.status,
            items=[
                schemas.OrderItemRead(
                    id=i.id,
                    itemordered_catalogitemid=i.itemordered_catalogitemid,
                    itemordered_productname=i.itemordered_productname,
                    itemordered_pictureuri=i.itemordered_pictureuri,
                    unitprice=float(i.unitprice),
                    units=i.units,
                )
                for i in o.items
            ],
            total=float(calculate_total(o.items))
        )
        for o in orders
    ]
