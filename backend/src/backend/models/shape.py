from datetime import datetime
from sqlalchemy import Integer, String, Float, ForeignKey
from sqlalchemy.orm import mapped_column, relationship, Mapped
from sqlalchemy_utils import EncryptedType
from backend.core.db import Base
from backend.core.encrypt import ENCRIPTION_KEY


class Shape(Base):
    __tablename__ = 'shapes'

    uuid = mapped_column(Integer, primary_key=True, autoincrement=True)
    date: Mapped[datetime] = mapped_column()
    weight = mapped_column(EncryptedType(Float, ENCRIPTION_KEY))
    height = mapped_column(EncryptedType(Float, ENCRIPTION_KEY))

    user_email = mapped_column(String, ForeignKey(
        'users.email', onupdate='CASCADE'))
    user = relationship('User', back_populates='shapes')
