from os import environ
from sqlalchemy import create_engine
from sqlalchemy.orm import DeclarativeBase, Session
from typing import Any, Generator

class Base(DeclarativeBase):
    pass 

ENGINE_PATH = f"postgresql://{environ.get('POSTGRES_USER')}:{environ.get('POSTGRES_PASSWORD')}@{environ.get('POSTGRES_HOST')}:5432"
engine = create_engine(ENGINE_PATH)
def get_db() -> Generator[Session, Any, None]:
    Base.metadata.create_all(engine)
    session = Session(engine)

    try:
        yield session
    except Exception as e:
        session.rollback()
        raise e
    finally:
        session.close()
