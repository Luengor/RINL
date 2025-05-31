from datetime import datetime
from sqlalchemy import Integer, String, Float, ForeignKey
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import StringEncryptedType
from backend.core.db import Base
from backend.core.encrypt import ENCRIPTION_KEY


class Activity(Base):
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
