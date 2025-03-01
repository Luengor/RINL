from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import Session
import pytest

from backend.main import app
from core.db import Base, get_db
from core.mail import get_send_email

@pytest.fixture
def db_engine():
    engine = create_engine("sqlite:///./test.db")
    Base.metadata.create_all(engine)
    yield engine
    Base.metadata.drop_all(engine)
    engine.dispose()

@pytest.fixture
def client(db_engine):
    def db():
        test_db = Session(db_engine)

        try:
            yield test_db
        except Exception as e:
            test_db.rollback()
            raise e
        finally:
            test_db.close()
            Base.metadata.drop_all(db_engine)

    # Database setup
    app.dependency_overrides[get_db] = db 

    # Mail setup
    app.dependency_overrides[get_send_email] = lambda: lambda to, subject, content: True

    # Create client
    return TestClient(app)
