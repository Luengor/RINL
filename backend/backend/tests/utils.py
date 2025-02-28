from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import Session
import pytest

from backend.main import app
from core.db import Base, get_db

def db():
    test_engine = create_engine("sqlite:///./test.db")

    Base.metadata.create_all(test_engine)
    test_db = Session(test_engine)

    try:
        yield test_db
    except Exception as e:
        test_db.rollback()
        raise e
    finally:
        test_db.close()
        Base.metadata.drop_all(test_engine)


@pytest.fixture
def client():
    # Database setup
    app.dependency_overrides[get_db] = db 

    # Create client
    return TestClient(app)
