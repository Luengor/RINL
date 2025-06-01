from datetime import datetime
from pydantic import BaseModel, ConfigDict
from backend.schemas.users import UserBase


class ShapeBase(BaseModel):
    """Base pydantic model for physical shape data."""
    model_config = ConfigDict(from_attributes=True)

    date: datetime
    """The date when the shape was recorded."""
    weight: float
    """The weight of the user in kilograms."""
    height: float
    """The height of the user in centimeters."""


class ShapeUUID(ShapeBase):
    """Pydantic model for physical shape data with an UUID."""

    uuid: int
    """The unique identifier for the shape record."""


class ShapeFull(ShapeUUID):
    """Pydantic model for full physical shape data including user information."""

    user: UserBase
    """The user whose physical shape is recorded."""
