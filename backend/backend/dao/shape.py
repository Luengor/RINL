from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError

from fastapi import HTTPException
from models.shape import Shape as ShapeModel
from schemas.shape import ShapeBase, ShapeFull
from schemas.users import UserBase
from datetime import datetime

class ShapeDAO:
    @staticmethod
    def create_shape(shape: ShapeBase, user: UserBase, session: Session) -> ShapeFull:
        try:
            shape_model = ShapeModel(
                date = shape.date,
                weight = shape.weight,
                height = shape.height,
                sex_math = shape.sex_math,
                user_email = user.email
            )
            
            session.add(shape_model)
            session.commit()

            return ShapeFull.model_validate(shape_model)
        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid shape")
        except Exception:
            raise HTTPException(status_code=500, detail="Internal server error")
    
    @staticmethod
    def get_shapes(email: str, from_date: datetime, to_date: datetime, session: Session) -> list[ShapeFull]:
        shapes = session.query(ShapeModel) \
            .filter(ShapeModel.user_email == email) \
            .filter(ShapeModel.date >= from_date) \
            .filter(ShapeModel.date <= to_date) \
            .all()
        return [ShapeFull.model_validate(shape) for shape in shapes]
    
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

