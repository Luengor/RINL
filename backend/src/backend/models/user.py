from typing import List, TYPE_CHECKING
from sqlalchemy import Integer, String, Boolean
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import StringEncryptedType
from backend.core.db import Base
from backend.core.encrypt import ENCRIPTION_KEY

if TYPE_CHECKING:
    from backend.models.activity import Activity
    from backend.models.shape import Shape


class User(Base):
    __tablename__ = 'users'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    email = mapped_column(String, unique=True)
    hashed_password = mapped_column(String)
    verified = mapped_column(Boolean, default=False)
    verification_code = mapped_column(String)
    name = mapped_column(StringEncryptedType(String, ENCRIPTION_KEY))
    year_of_birth = mapped_column(StringEncryptedType(Integer, ENCRIPTION_KEY))

    activities: Mapped[List["Activity"]] = relationship(
        "Activity", cascade="all, delete")  # type: ignore
    shapes: Mapped[List["Shape"]] = relationship(
        "Shape", cascade="all, delete")  # type: ignore
