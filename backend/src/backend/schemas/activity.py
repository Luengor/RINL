"""Activity model for validating and serializing activity data."""

from datetime import datetime
from pydantic import BaseModel, ConfigDict
from backend.schemas.users import UserBase


class ActivityBase(BaseModel):
    """Base pydantic model for activity data."""

    model_config = ConfigDict(from_attributes=True)

    date: datetime
    """The date when the activity was recorded."""

    minigame: str
    """The name of the minigame played."""

    duration: float
    """The duration of the activity in seconds."""

    score: float
    """The score achieved in the activity."""

    activity_points: float
    """The activity points earned from the activity."""

    extra_data: str
    """Additional data related to the activity, stored as a JSON string."""


class ActivityUUID(ActivityBase):
    """Pydantic model for activity data with UUID."""

    uuid: int
    """The unique identifier for the activity."""


class ActivityFull(ActivityUUID):
    """Pydantic model for full activity data including user information."""

    user: UserBase
    """The user who performed the activity."""
