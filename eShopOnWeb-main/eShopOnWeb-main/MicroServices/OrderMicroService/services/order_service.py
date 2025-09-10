from OrderMicroService.models.order import Order
from OrderMicroService.repositories.order_repository import OrderRepository

class OrderService:
    def __init__(self, order_repo: OrderRepository):
        self.order_repo = order_repo

    async def create_order(self, buyer_id: str, shipping_address, order_items):
        order = Order(
            buyer_id=buyer_id,
            ship_to_address=shipping_address,
            order_items=order_items
        )
        return await self.order_repo.add(order)
