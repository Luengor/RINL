from fastapi import APIRouter, Depends

from core.auth import get_current_verified_user
from core.db import get_db
from schemas.shape import ShapeBase, ShapeFull, ShapeBase
from schemas.users import UserBase
from dao.shape import ShapeDAO

from datetime import datetime, timezone

router = APIRouter(
    prefix="/shape",
    tags=["shape"]
)

@router.post("/", response_model=ShapeFull)
async def create_shape(shape: ShapeBase, user: UserBase = Depends(get_current_verified_user), session=Depends(get_db)):
    # Create shape
    return ShapeDAO.create_shape(shape, user, session)

@router.get("/", response_model=list[ShapeFull])
async def get_shapes(user: UserBase = Depends(get_current_verified_user), from_date: str | None = None, to_date: str | None = None, session=Depends(get_db)):
    # Convert dates to datetime 
    from_date_dt = datetime.fromisoformat(from_date) if from_date is not None else datetime.min.replace(tzinfo=timezone.utc)
    to_date_dt = datetime.fromisoformat(to_date) if to_date is not None else datetime.max.replace(tzinfo=timezone.utc)

    return ShapeDAO.get_shapes(user.email, from_date_dt, to_date_dt, session)

@router.get("/current", response_model=ShapeFull)
async def get_current_shape(user: UserBase = Depends(get_current_verified_user), session=Depends(get_db)):
    return ShapeDAO.get_current_shape(user.email, session)

@router.delete("/{shape_id}", response_model=ShapeBase)
async def delete_shape(shape_id: int, user: UserBase = Depends(get_current_verified_user), session=Depends(get_db)):
    # Delete shape
    return ShapeDAO.delete_shape(shape_id, user.email, session)
