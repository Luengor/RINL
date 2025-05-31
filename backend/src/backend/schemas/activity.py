from datetime import datetime
from pydantic import BaseModel, ConfigDict
from backend.schemas.users import UserBase


class ActivityBase(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    date: datetime
    minigame: str
    duration: float
    score: float
    activity_points: float
    extra_data: str


class ActivityUUID(ActivityBase):
    uuid: int


class ActivityFull(ActivityUUID):
    user: UserBase
