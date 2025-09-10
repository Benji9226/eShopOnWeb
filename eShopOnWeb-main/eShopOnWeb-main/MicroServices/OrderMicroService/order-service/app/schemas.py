from datetime import datetime
from pydantic import BaseModel
from typing import List, Optional
from decimal import Decimal

# Input schema for order items
class OrderItemCreate(BaseModel):
    itemordered_catalogitemid: Optional[int]
    itemordered_productname: Optional[str]
    itemordered_pictureuri: Optional[str]
    unitprice: Decimal
    units: int

# Input schema for creating an order
class OrderCreate(BaseModel):
    buyer_id: str
    shiptoaddress_street: Optional[str]
    shiptoaddress_city: Optional[str]
    shiptoaddress_state: Optional[str]
    shiptoaddress_country: Optional[str]
    shiptoaddress_zipcode: Optional[str]
    items: List[OrderItemCreate]

# Output schema for order items
class OrderItemRead(OrderItemCreate):
    id: int

# Output schema for orders
class OrderRead(BaseModel):
    id: int
    buyer_id: str
    order_date: datetime  # <- changed to datetime
    shiptoaddress_street: Optional[str]
    shiptoaddress_city: Optional[str]
    shiptoaddress_state: Optional[str]
    shiptoaddress_country: Optional[str]
    shiptoaddress_zipcode: Optional[str]
    status: str
    items: List[OrderItemRead]

    class Config:
        from_attributes = True  # v2 replacement for orm_mode
