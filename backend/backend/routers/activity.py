from fastapi import APIRouter, Depends, HTTPException

from core.auth import get_current_verified_user
from schemas.activity import ActivityFull, ActivityBase, ActivityUUID
from schemas.users import UserBase
from dao.activity import ActivityDAO

from datetime import datetime, timezone

router = APIRouter(
    prefix="/activity",
    tags=["activity"]
)

@router.post("/", response_model=ActivityFull)
async def create_activity(activity: ActivityBase, user: UserBase = Depends(get_current_verified_user)):
    # Create activity
    return ActivityDAO.create_activity(activity, user)

@router.get("/", response_model=list[ActivityUUID])
async def get_activities(user: UserBase = Depends(get_current_verified_user), minigame_filter: str | None = None, from_date: str | None = None, to_date: str | None = None):
    # Convert dates to datetime 
    from_date_dt = datetime.fromisoformat(from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ActivityDAO.get_activities(user.email, minigame_filter, from_date_dt, to_date_dt)

@router.delete("/{activity_id}", response_model=ActivityBase)
async def delete_activity(activity_id: int, user: UserBase = Depends(get_current_verified_user)):
    # Delete activity
    return ActivityDAO.delete_activity(activity_id, user.email)

