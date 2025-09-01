class BaseEntity:
    # Stub for inheritance; expand with actual fields/logic as needed
    pass


class OrderItem(BaseEntity):
    def __init__(self, item_ordered, unit_price: float, units: int):
        self._item_ordered = item_ordered
        self._unit_price = unit_price
        self._units = units

    @property
    def item_ordered(self):
        return self._item_ordered

    @property
    def unit_price(self) -> float:
        return self._unit_price

    @property
    def units(self) -> int:
        return self._units

    def __repr__(self):
        return (f"OrderItem(item_ordered={self._item_ordered}, "
                f"unit_price={self._unit_price}, units={self._units})")
