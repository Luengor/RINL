from fastapi.testclient import TestClient
from sqlalchemy import create_engine, Engine
from sqlalchemy.orm import Session
from typing import Generator
import pytest

from backend.main import app
from core.db import Base, get_db
from core.mail import get_send_email
from core.auth_utils import get_password_hash, create_access_token
from models.user import User
from .data import test_user

@pytest.fixture(scope="session", autouse=True)
def engine() -> Engine:
    engine = create_engine("sqlite:///./test.db")
    Base.metadata.drop_all(engine)
    Base.metadata.create_all(engine)

    return engine

@pytest.fixture()
def session(engine: Engine) -> Generator[Session, None, None]:
    with engine.connect() as connection:
        connection.begin()
        session = Session(connection)

        yield session

        session.close()
        connection.rollback()


@pytest.fixture()
def client(session: Session):
    # Database setup
    app.dependency_overrides[get_db] = lambda: session

    # Mail setup
    app.dependency_overrides[get_send_email] = lambda: lambda to, subject, content: True

    # Create client
    client = TestClient(app)
    yield client

    app.dependency_overrides.clear()

@pytest.fixture
def login_token(session: Session):
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

