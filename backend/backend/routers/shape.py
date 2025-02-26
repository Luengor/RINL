from fastapi import APIRouter, Depends, HTTPException

from core.auth import get_current_verified_user
from schemas.shape import ShapeBase, ShapeFull, ShapeBase, ShapeUUID
from schemas.users import UserBase
from dao.shape import ShapeDAO

from datetime import datetime, timezone

router = APIRouter(
    prefix="/shape",
    tags=["shape"]
)

@router.post("/", response_model=ShapeFull)
async def create_shape(shape: ShapeBase, user: UserBase = Depends(get_current_verified_user)):
    # Create shape
    return ShapeDAO.create_shape(shape, user)

@router.get("/", response_model=list[ShapeUUID])
async def get_shapes(user: UserBase = Depends(get_current_verified_user), from_date: str | None = None, to_date: str | None = None):
    # Convert dates to datetime 
    from_date_dt = datetime.fromisoformat(from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ShapeDAO.get_shapes(user.email, from_date_dt, to_date_dt)

@router.delete("/{shape_id}", response_model=ShapeBase)
async def delete_shape(shape_id: int, user: UserBase = Depends(get_current_verified_user)):
    # Delete shape
    return ShapeDAO.delete_shape(shape_id, user.email)
