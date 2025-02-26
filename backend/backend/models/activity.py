from sqlalchemy.orm import mapped_column, relationship
from sqlalchemy import Date, Integer, String, Float, ForeignKey
from core.db import Base

class Activity(Base):
    __tablename__ = 'activities'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date = mapped_column(Date)
    user_email = mapped_column(String, ForeignKey('users.email'))
    minigame = mapped_column(String)
    duration = mapped_column(Float)
    activity_points = mapped_column(Float)
    extra_data = mapped_column(String, default="{}")  # JSON

    user = relationship('User', back_populates='activities')
