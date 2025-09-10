from pydantic import BaseModel
from typing import List, Optional
from decimal import Decimal


class OrderItemCreate(BaseModel):
itemordered_catalogitemid: Optional[int]
itemordered_productname: Optional[str]
itemordered_pictureuri: Optional[str]
unitprice: Decimal
units: int


class OrderCreate(BaseModel):
buyer_id: str
shiptoaddress_street: Optional[str]
shiptoaddress_city: Optional[str]
shiptoaddress_state: Optional[str]
shiptoaddress_country: Optional[str]
shiptoaddress_zipcode: Optional[str]
items: List[OrderItemCreate]


class OrderItemRead(OrderItemCreate):
id: int


class OrderRead(BaseModel):
id: int
buyer_id: str
order_date: str
shiptoaddress_street: Optional[str]
shiptoaddress_city: Optional[str]
shiptoaddress_state: Optional[str]
shiptoaddress_country: Optional[str]
shiptoaddress_zipcode: Optional[str]
status: str
items: List[OrderItemRead]


class Config:
orm_mode = True