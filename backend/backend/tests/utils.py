from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import Session
import pytest

from backend.main import app
from core.db import Base, get_db
from core.mail import get_send_email

engine = create_engine("sqlite:///./test.db")
Base.metadata.drop_all(engine)
Base.metadata.create_all(engine)
session = Session(engine)

@pytest.fixture
def client():
    def db():
        return session

    # Database setup
    app.dependency_overrides[get_db] = db 

    # Mail setup
    app.dependency_overrides[get_send_email] = lambda: lambda to, subject, content: True

    # Create client
    return TestClient(app)
