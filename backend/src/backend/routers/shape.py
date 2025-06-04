"""Shape API router for managing user physical shapes."""

from datetime import datetime, timezone
from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from backend.core.auth import get_current_verified_user
from backend.core.db import get_db
from backend.dao.shape import ShapeDAO
from backend.schemas.shape import ShapeBase, ShapeFull
from backend.schemas.users import UserBase


router = APIRouter(
    prefix="/shape",
    tags=["shape"]
)


@router.post("/", response_model=ShapeFull)
async def create_shape(shape: ShapeBase, user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    """Create a new shape for the user.

    This function is an endpoint.

    Args:
        shape (ShapeBase): The shape data to create.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        ShapeFull: The created shape with full details.
    """
    # Create shape
    return ShapeDAO.create_shape(shape, user, session)


@router.get("/", response_model=list[ShapeFull])
async def get_shapes(user: UserBase = Depends(get_current_verified_user), from_date: str | None = None, to_date: str | None = None, session: Session = Depends(get_db)):
    """Get all shapes for the user within a date range.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
        from_date (str | None): The start date in ISO format (optional).
        to_date (str | None): The end date in ISO format (optional).
        session (Session): The database session, automatically injected by FastAPI.

    Returns:
        list[ShapeFull]: A list of shapes for the user within the specified date range.
    """
    # Convert dates to datetime
    from_date_dt = datetime.fromisoformat(
        from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(
        to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ShapeDAO.get_shapes(user.email, from_date_dt, to_date_dt, session)


@router.get("/current", response_model=ShapeFull)
async def get_current_shape(user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    """Get the current shape for the user.

    This function is an endpoint.

    Args:
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        ShapeFull: The current shape for the user.
    """
    return ShapeDAO.get_current_shape(user.email, session)


@router.delete("/{shape_id}", response_model=ShapeBase)
async def delete_shape(shape_id: int, user: UserBase = Depends(get_current_verified_user), session: Session = Depends(get_db)):
    """Delete a shape by its ID.

    This function is an endpoint.

    Args:
        shape_id (int): The ID of the shape to delete.
        user (UserBase): The current user, automatically injected by FastAPI.
        session (Session): The database session, automatically injected by FastAPI.
    Returns:
        ShapeBase: The deleted shape data.
    """
    # Delete shape
    return ShapeDAO.delete_shape(shape_id, user.email, session)
