from pydantic import BaseModel, ConfigDict
from datetime import datetime

from .users import UserBase 

class ShapeBase(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    date: datetime
    weight: float
    height: float
    sex_math: float

class ShapeUUID(ShapeBase):
    uuid: int

class ShapeFull(ShapeUUID):
    user: UserBase

