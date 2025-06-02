"""Activity model for tracking user activities in the system."""

from datetime import datetime
from sqlalchemy import Integer, String, Float, ForeignKey
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import StringEncryptedType
from backend.core.db import Base
from backend.core.encrypt import ENCRIPTION_KEY


class Activity(Base):
    """Model representing a user's activity in the system.

    Attributes:
        uuid (int): Auto-incrementing integer used as the primary key. 
        date (datetime): The date and time when the activity occurred.
        user_email (str): Email of the user associated with the activity.
        minigame (str): Name of the minigame played during the activity.
        duration (float): Duration of the activity in seconds, encrypted.
        activity_points (float): Points earned during the activity, encrypted.
        score (float): Score achieved in the activity.
        extra_data (str): Additional data related to the activity, stored as a JSON string and encrypted.

        user (relationship): Relationship to the User model, allowing access to user details associated with the activity.
    """
    __tablename__ = 'activities'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date: Mapped[datetime] = mapped_column()
    user_email = mapped_column(String, ForeignKey(
        'users.email', onupdate="CASCADE"))
    minigame = mapped_column(String)
    duration = mapped_column(StringEncryptedType(Float, ENCRIPTION_KEY))
    score = mapped_column(Float)
    activity_points = mapped_column(StringEncryptedType(Float, ENCRIPTION_KEY))
    extra_data = mapped_column(StringEncryptedType(
        String, ENCRIPTION_KEY), default="{}")  # JSON

    user = relationship('User', back_populates='activities')
