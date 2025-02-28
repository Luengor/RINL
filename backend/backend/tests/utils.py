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

    print("hola", flush=True)
    try:
        yield test_db
    finally:
        test_db.close()
        Base.metadata.drop_all(test_engine)


@pytest.fixture
def client():
    # Database setup
    app.dependency_overrides[get_db] = db 

    # Remove lifespan

    # Create client
    with TestClient(app) as client:
        yield client
