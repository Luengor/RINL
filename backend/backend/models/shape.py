from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy import Integer, String, Float, ForeignKey
from core.db import Base
from datetime import datetime

class Shape(Base):
    __tablename__ = 'shapes'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date: Mapped[datetime] = mapped_column()
    weight = mapped_column(Float)
    height = mapped_column(Float)
    sex_math = mapped_column(Float)

    user_email = mapped_column(String, ForeignKey('users.email', onupdate='CASCADE'))
    user = relationship('User', back_populates='shapes')


