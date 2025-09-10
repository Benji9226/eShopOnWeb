from pydantic import BaseModel

class Address(BaseModel):
    street: str
    city: str
    postal_code: str
    country: str

class CatalogItemOrdered(BaseModel):
    item_id: int
    name: str
    picture_uri: str
