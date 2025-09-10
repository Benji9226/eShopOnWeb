from typing import List
from datetime import datetime
from .order_item import OrderItem
from .value_objects import Address
from pydantic import BaseModel
from typing import Optional


class Order(BaseModel):
    id: Optional[int] = None
    buyer_id: str
    order_date: datetime = datetime.utcnow()
    ship_to_address: Address
    order_items: List[OrderItem]

    def total(self) -> float:
        return sum(item.unit_price * item.units for item in self.order_items)
