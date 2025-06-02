"""Base database model and session management dependencies."""

from os import environ
from typing import Any, Generator
from sqlalchemy import create_engine
from sqlalchemy.orm import DeclarativeBase, Session


class Base(DeclarativeBase):
    """Base class for all SQLAlchemy models."""


ENGINE_PATH = f"postgresql://{environ.get('POSTGRES_USER')}:{environ.get('POSTGRES_PASSWORD')}@{environ.get('POSTGRES_HOST')}:5432"
engine = create_engine(ENGINE_PATH)


def get_db() -> Generator[Session, Any, None]:
    """Dependency that provides a database session.

    The yielded session is automatically committed and closed after use.
    If an exception occurs, the session is rolled back.

    Returns:
        Generator[Session, Any, None]: A generator that yields a SQLAlchemy session.
    """
    Base.metadata.create_all(engine)
    session = Session(engine)

    try:
        yield session
    except Exception as e:
        session.rollback()
        raise e
    finally:
        session.close()
