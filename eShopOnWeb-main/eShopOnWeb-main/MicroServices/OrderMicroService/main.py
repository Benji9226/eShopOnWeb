from fastapi import FastAPI
from OrderMicroService.models.order_item import OrderItem
from OrderMicroService.models.value_objects import Address, CatalogItemOrdered
from OrderMicroService.services.order_service import OrderService
from OrderMicroService.repositories.order_repository import OrderRepository

app = FastAPI()
order_repo = OrderRepository()
order_service = OrderService(order_repo)

@app.post("/orders")
async def create_order(buyer_id: str, address: Address, items: list[OrderItem]):
    order = await order_service.create_order(buyer_id, address, items)
    return {"order_id": order.id, "total": order.total()}
