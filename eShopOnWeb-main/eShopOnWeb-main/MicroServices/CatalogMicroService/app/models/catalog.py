from pydantic import BaseModel

class Catalog(BaseModel):
    id: int
    product: str
    quantity: int