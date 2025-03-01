from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import Session
import pytest

from backend.main import app
from core.db import Base, get_db
from core.mail import get_send_email
from core.auth_utils import get_password_hash, create_access_token
from models.user import User
from .data import test_user

# Database setup
engine = create_engine("sqlite:///./test.db")
session = Session(engine)

@pytest.fixture
def client():
    global session, engine

    def db():
        return session

    # I don't like this
    session.close()
    Base.metadata.drop_all(engine)
    Base.metadata.create_all(engine)
    session = Session(engine)

    # Database setup
    app.dependency_overrides[get_db] = db 

    # Mail setup
    app.dependency_overrides[get_send_email] = lambda: lambda to, subject, content: True

    # Create client
    return TestClient(app)

@pytest.fixture
def login_token(client):
    # Add user to database
    test_user_dict = test_user.model_dump()
    test_user_dict["hashed_password"] = get_password_hash(test_user.password) 
    test_user_dict.pop("password")
    test_user_dict["verified"] = True

    user = User(**test_user_dict)
    session.add(user)
    session.commit()

    # Create login token
    return create_access_token({"sub": user.email})

