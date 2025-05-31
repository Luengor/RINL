from datetime import datetime
from pydantic import BaseModel, ConfigDict
from backend.schemas.users import UserBase


class ShapeBase(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    date: datetime
    weight: float
    height: float


class ShapeUUID(ShapeBase):
    uuid: int


class ShapeFull(ShapeUUID):
    user: UserBase
