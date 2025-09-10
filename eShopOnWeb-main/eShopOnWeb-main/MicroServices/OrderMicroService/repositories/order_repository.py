from typing import List
from OrderMicroService.models.order import Order

# For simplicity, using in-memory storage
class OrderRepository:
    _orders: List[Order] = []

    async def add(self, order: Order):
        order.id = len(self._orders) + 1
        self._orders.append(order)
        return order

    async def list_all(self) -> List[Order]:
        return self._orders
