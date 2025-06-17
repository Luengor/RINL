"""Data Access Object for handling shapes in the database."""

# pylint: disable=raise-missing-from
from datetime import datetime
from functools import lru_cache
from sqlalchemy.orm import Session
from sqlalchemy.exc import IntegrityError
from fastapi import HTTPException
from backend.models.shape import Shape as ShapeModel
from backend.schemas.shape import ShapeBase, ShapeFull
from backend.schemas.users import UserBase


class ShapeDAO:
    """Data Access Object for handling shapes in the database."""

    @staticmethod
    def create_shape(shape: ShapeBase, user: UserBase, session: Session) -> ShapeFull:
        """Create a new shape in the database.

        Args:
            shape (ShapeBase): The shape data to create.
            user (UserBase): The user associated with the shape.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            ShapeFull: The created shape with full details.

        Raises:
            HTTPException: If the shape is invalid or if there is an internal server error.
        """
        try:
            # Use current date if the given one is timezone naive
            if shape.date.tzinfo is None:
                shape.date = shape.date.replace(
                    tzinfo=datetime.now().astimezone().tzinfo)

            shape_model = ShapeModel(
                date=shape.date,
                weight=shape.weight,
                height=shape.height,
                user_email=user.email
            )

            session.add(shape_model)
            session.commit()

            # Clear the lru_cache for the get function
            ShapeDAO.get_shapes.cache_clear()

            return ShapeFull.model_validate(shape_model)
        except IntegrityError:
            raise HTTPException(status_code=400, detail="Invalid shape")
        except Exception:
            raise HTTPException(
                status_code=500, detail="Internal server error")

    @staticmethod
    @lru_cache(maxsize=128)
    def get_shapes(email: str, from_date: datetime, to_date: datetime, session: Session) -> list[ShapeFull]:
        """Retrieve shapes for a user within a date range.

        This method is cached to improve performance for frequently accessed data.
        The cache is cleared whenever a new shape is created or deleted.

        Args:
            email (str): The email of the user whose shapes are to be retrieved.
            from_date (datetime): The start date of the range.
            to_date (datetime): The end date of the range.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            list[ShapeFull]: A list of shapes for the user within the specified date range.
        """
        shapes = session.query(ShapeModel) \
            .filter(ShapeModel.user_email == email) \
            .filter(ShapeModel.date >= from_date) \
            .filter(ShapeModel.date <= to_date) \
            .order_by(ShapeModel.date) \
            .all()
        return [ShapeFull.model_validate(shape) for shape in shapes]

    @staticmethod
    def get_current_shape(email: str, session: Session) -> ShapeFull:
        """Retrieve the most recent shape for a user.

        Args:
            email (str): The email of the user whose current shape is to be retrieved.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            ShapeFull: The most recent shape for the user.

        Raises:
            HTTPException: If no shape is found for the user.
        """
        shape = session.query(ShapeModel) \
            .filter(ShapeModel.user_email == email) \
            .order_by(ShapeModel.date.desc()) \
            .first()

        if shape is None:
            raise HTTPException(status_code=404, detail="Shape not found")

        return ShapeFull.model_validate(shape)

    @staticmethod
    def delete_shape(shape_id: int, user_email: str, session: Session) -> ShapeFull:
        """Delete a shape by its ID and user email.

        Args:
            shape_id (int): The ID of the shape to delete.
            user_email (str): The email of the user who owns the shape.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            ShapeFull: The deleted shape details.

        Raises:
            HTTPException: If the shape is not found or if there is an internal server error.
        """
        shape = session.query(ShapeModel) \
            .filter(ShapeModel.uuid == shape_id) \
            .filter(ShapeModel.user_email == user_email) \
            .first()

        if shape is None:
            raise HTTPException(status_code=404, detail="Shape not found")

        # Clear the lru_cache for the get function
        ShapeDAO.get_shapes.cache_clear()

        model = ShapeFull.model_validate(shape)
        session.delete(shape)
        session.commit()

        return model
