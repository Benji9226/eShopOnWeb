from datetime import datetime
from decimal import Decimal
from typing import List, Tuple
from Model import Address, OrderItem


class BaseEntity:
    # Stub for inheritance; expand with actual fields/logic as needed
    pass


class IAggregateRoot:
    # Marker interface stub — not necessary in Python, but included for parity
    pass


class Order(BaseEntity, IAggregateRoot):
    def __init__(self, buyer_id: str, ship_to_address: Address, items: List[OrderItem.BaseEntity]):
        if not buyer_id or buyer_id.strip() == "":
            raise ValueError("buyer_id cannot be null or empty")

        self._buyer_id = buyer_id
        self._ship_to_address = ship_to_address
        self._order_date = datetime.now()
        self._order_items: List[OrderItem.BaseEntity] = items or []

    @property
    def buyer_id(self) -> str:
        return self._buyer_id

    @property
    def order_date(self) -> datetime:
        return self._order_date

    @property
    def ship_to_address(self):
        return self._ship_to_address

    @property
    def order_items(self) -> Tuple[OrderItem.BaseEntity, ...]:
        # Returns a read-only tuple to prevent external mutation
        return tuple(self._order_items)

    def add_order_item(self, order_item: "OrderItem"):
        """Encapsulated method to add items, enforcing DDD rules."""
        self._order_items.append(order_item)

    def total(self) -> Decimal:
        total = Decimal("0.0")
        for item in self._order_items:
            total += Decimal(item.unit_price) * item.units
        return total

    def __repr__(self):
        return (f"Order(buyer_id='{self._buyer_id}', order_date={self._order_date}, "
                f"ship_to_address={self._ship_to_address}, order_items={self._order_items})")
