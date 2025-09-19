from sqlmodel import SQLModel, Field

class BaseEntity(SQLModel):
    id: int = Field(default=None, primary_key=True)
