"""User model for storing in the system."""

from typing import List, TYPE_CHECKING
from sqlalchemy import Integer, String, Boolean
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import StringEncryptedType
from backend.core.db import Base
from backend.core.common import ENCRIPTION_KEY

if TYPE_CHECKING:
    from backend.models.activity import Activity
    from backend.models.shape import Shape


class User(Base):
    """Model representing a user in the system.

    Attributes:
        uuid (int): Auto-incrementing integer used as the primary key.
        email (str): Unique email address of the user.
        hashed_password (str): Hashed password for user authentication.
        verified (bool): Indicates whether the user's email is verified.
        name (str): Encrypted name of the user.
        year_of_birth (int): Encrypted year of birth of the user.

        activities (list[Activity]): List of activities associated with the user.
        shapes (list[Shape]): List of physical shapes associated with the user.
    """
    __tablename__ = 'users'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    email = mapped_column(String, unique=True)
    hashed_password = mapped_column(String)
    verified = mapped_column(Boolean, default=False)
    name = mapped_column(StringEncryptedType(String, ENCRIPTION_KEY))
    year_of_birth = mapped_column(StringEncryptedType(Integer, ENCRIPTION_KEY))

    activities: Mapped[List["Activity"]] = relationship(
        "Activity", cascade="all, delete")  # type: ignore
    shapes: Mapped[List["Shape"]] = relationship(
        "Shape", cascade="all, delete")  # type: ignore
