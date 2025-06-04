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
    """Create a new activity for the user.

    This function is an endpoint.

    Args:
        activity (ActivityBase): The activity data to create.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        ActivityFull: The created activity with full details.
    """
    # Create activity
    return ActivityDAO.create_activity(activity, user, session)


@router.get("/", response_model=list[ActivityUUID])
async def get_activities(user: UserBase = Depends(get_current_verified_user), minigame_filter: str | None = None, from_date: str | None = None, to_date: str | None = None, session: Session = Depends(get_db)):
    """Get all activities for the user within a date range and optional minigame filter.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
        minigame_filter (str | None): The minigame filter (optional).
        from_date (str | None): The start date in ISO format (optional).
        to_date (str | None): The end date in ISO format (optional).
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        list[ActivityUUID]: A list of activities for the user within the specified date range and filter.
    """
    # Convert dates to datetime
    from_date_dt = datetime.fromisoformat(
        from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(
        to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ActivityDAO.get_activities(user.email, minigame_filter, from_date_dt, to_date_dt, session)


@router.delete("/{activity_id}", response_model=ActivityBase)
async def delete_activity(activity_id: int, user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    """Delete an activity by its ID.

    This function is an endpoint.

    Args:
        activity_id (int): The ID of the activity to delete.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        ActivityBase: The deleted activity details.
    """
    # Delete activity
    return ActivityDAO.delete_activity(activity_id, user.email, session)
