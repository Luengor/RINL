from sqlalchemy.orm import Session
from core.db import engine

from models.shape import Shape as ShapeModel
from schemas.shape import ShapeBase, ShapeFull
from schemas.users import UserBase
from datetime import datetime

class ShapeDAO:
    @staticmethod
    def create_shape(shape: ShapeBase, user: UserBase) -> ShapeFull | None:
        with Session(engine) as session:
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
    
    @staticmethod
    def get_shapes(email: str, from_date: datetime, to_date: datetime) -> list[ShapeFull]:
        with Session(engine) as session:
            shapes = session.query(ShapeModel) \
                .filter(ShapeModel.user_email == email) \
                .filter(ShapeModel.date >= from_date) \
                .filter(ShapeModel.date <= to_date) \
                .all()
            return [ShapeFull.model_validate(shape) for shape in shapes]
    
    @staticmethod
    def delete_shape(shape_id: int, user_email: str) -> ShapeFull | None:
        with Session(engine) as session:
            shape = session.query(ShapeModel) \
                .filter(ShapeModel.uuid == shape_id) \
                .filter(ShapeModel.user_email == user_email) \
                .first()
            if shape is None:
                return None

            model = ShapeFull.model_validate(shape)
            session.delete(shape)
            session.commit()

            return model

