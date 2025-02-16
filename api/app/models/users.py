from typing import List
from sqlalchemy import Integer, String, Boolean
from sqlalchemy.orm import Mapped, mapped_column, relationship
from core.db import Base

class User(Base):
    __tablename__ = 'users'

    email = mapped_column(String, primary_key=True)
    hashed_password = mapped_column(String)
    verified_email = mapped_column(Boolean, default=False)
    name = mapped_column(String)
    year_of_birth = mapped_column(Integer)

    # activities : Mapped[List["Activity"]] = relationship(back_populates='user_email') # type: ignore

