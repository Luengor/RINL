from sqlalchemy.orm import mapped_column, relationship
from sqlalchemy import Date, String, Float, ForeignKey
from core.db import Base

class Activity(Base):
    __tablename__ = 'activities'

    date = mapped_column(Date, primary_key=True)
    user_email = mapped_column(String, ForeignKey('users.email'), primary_key=True)
    minigame = mapped_column(String)
    duration = mapped_column(Float)
    activity_points = mapped_column(Float)
    extra_data = mapped_column(String)  # JSON
