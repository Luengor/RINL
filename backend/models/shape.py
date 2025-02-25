from sqlalchemy.orm import mapped_column, relationship
from sqlalchemy import Date, Integer, String, Float, ForeignKey
from core.db import Base

class Shape(Base):
    __tablename__ = 'shapes'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date = mapped_column(Date)
    weight = mapped_column(Float)
    height = mapped_column(Float)
    sex_math = mapped_column(Float)

    user_email = mapped_column(String, ForeignKey('users.email'))
    user = relationship('User', back_populates='shapes')


