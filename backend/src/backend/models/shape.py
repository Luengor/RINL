"""Shape model for tracking user physical shape in the system."""

from datetime import datetime
from sqlalchemy import Integer, String, Float, ForeignKey
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import StringEncryptedType
from backend.core.db import Base
from backend.core.common import ENCRIPTION_KEY


class Shape(Base):
    """Model representing the physical shape of a user.

    Attributes:
        uuid (int): Auto-incrementing integer used as the primary key.
        date (datetime): The date and time when the shape was recorded.
        weight (float): The user's weight, encrypted.
        height (float): The user's height, encrypted.
        user_email (str): Email of the user associated with the shape.

        user (relationship): Relationship to the User model.
    """

    __tablename__ = 'shapes'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date: Mapped[datetime] = mapped_column()
    weight = mapped_column(StringEncryptedType(Float, ENCRIPTION_KEY))
    height = mapped_column(StringEncryptedType(Float, ENCRIPTION_KEY))

    user_email = mapped_column(String, ForeignKey(
        'users.email', onupdate='CASCADE'))
    user = relationship('User', back_populates='shapes')
