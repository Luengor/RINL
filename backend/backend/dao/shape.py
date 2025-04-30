from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError

from fastapi import HTTPException
from models.shape import Shape as ShapeModel
from schemas.shape import ShapeBase, ShapeFull
from schemas.users import UserBase
from datetime import datetime
from functools import lru_cache

class ShapeDAO:
    @staticmethod
    def create_shape(shape: ShapeBase, user: UserBase, session: Session) -> ShapeFull:
        try:
            shape_model = ShapeModel(
                date = shape.date,
                weight = shape.weight,
                height = shape.height,
                user_email = user.email
            )
            
            session.add(shape_model)
            session.commit()

            # Clear the lru_cache for the get_activities method
            ShapeDAO.get_shapes.cache_clear()

            return ShapeFull.model_validate(shape_model)
        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid shape")
        except Exception:
            raise HTTPException(status_code=500, detail="Internal server error")
    
    @staticmethod
    @lru_cache(maxsize=128)
    def get_shapes(email: str, from_date: datetime, to_date: datetime, session: Session) -> list[ShapeFull]:
        shapes = session.query(ShapeModel) \
            .filter(ShapeModel.user_email == email) \
            .filter(ShapeModel.date >= from_date) \
            .filter(ShapeModel.date <= to_date) \
            .order_by(ShapeModel.date) \
            .all()
        return [ShapeFull.model_validate(shape) for shape in shapes]
    
    @staticmethod
    def get_current_shape(email: str, session: Session) -> ShapeFull:
        shape = session.query(ShapeModel) \
            .filter(ShapeModel.user_email == email) \
            .order_by(ShapeModel.date.desc()) \
            .first()

        if shape is None:
            raise HTTPException(status_code=404, detail="Shape not found")

        return ShapeFull.model_validate(shape)

    @staticmethod
    def delete_shape(shape_id: int, user_email: str, session: Session) -> ShapeFull:
        shape = session.query(ShapeModel) \
            .filter(ShapeModel.uuid == shape_id) \
            .filter(ShapeModel.user_email == user_email) \
            .first()

        if shape is None:
            raise HTTPException(status_code=404, detail="Shape not found")

        model = ShapeFull.model_validate(shape)
        session.delete(shape)
        session.commit()

        return model

