"""Activity API router for managing user activities."""

from datetime import datetime, timezone
from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends
from backend.core.auth import get_current_verified_user
from backend.core.db import get_db
from backend.dao.activity import ActivityDAO
from backend.schemas.activity import ActivityFull, ActivityBase, ActivityUUID
from backend.schemas.users import UserBase


router = APIRouter(
    prefix="/activity",
    tags=["activity"]
)


@router.post("/", response_model=ActivityFull)
async def create_activity(activity: ActivityBase, user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    # Create activity
    return ActivityDAO.create_activity(activity, user, session)


@router.get("/", response_model=list[ActivityUUID])
async def get_activities(user: UserBase = Depends(get_current_verified_user), minigame_filter: str | None = None, from_date: str | None = None, to_date: str | None = None, session: Session = Depends(get_db)):
    # Convert dates to datetime
    from_date_dt = datetime.fromisoformat(
        from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(
        to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ActivityDAO.get_activities(user.email, minigame_filter, from_date_dt, to_date_dt, session)


@router.delete("/{activity_id}", response_model=ActivityBase)
async def delete_activity(activity_id: int, user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    # Delete activity
    return ActivityDAO.delete_activity(activity_id, user.email, session)
