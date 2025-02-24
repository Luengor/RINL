from sqlalchemy import Integer, String, Boolean 
from sqlalchemy.orm import mapped_column, relationship, Mapped
from core.db import Base
from typing import List

from .activity import Activity

class User(Base):
    __tablename__ = 'users'

    email = mapped_column(String, primary_key=True)
    hashed_password = mapped_column(String)
    verified = mapped_column(Boolean, default=False)
    verification_code = mapped_column(String)
    name = mapped_column(String)
    year_of_birth = mapped_column(Integer)

    activities: Mapped[List["Activity"]] = relationship("Activity") # type: ignore

