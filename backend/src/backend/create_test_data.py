"""Utility module to create some test data in the database for development purposes."""

from datetime import datetime, timedelta, timezone
from os import environ
import random
from sqlalchemy import create_engine
from sqlalchemy.orm import Session
from backend.core.auth_utils import get_password_hash
from backend.core.db import Base
from backend.models.user import User
from backend.models.shape import Shape
from backend.models.activity import Activity

POSTGRES_USER = environ.get("POSTGRES_USER", "postgres")
POSTGRES_PASSWORD = environ.get("POSTGRES_PASSWORD", "postgres")
POSTGRES_HOST = environ.get("POSTGRES_HOST", "localhost")

ACTIVITY_COUNT = 2000
ACTIVITIES_FROM = 100
MINIGAMES = ["Globos", "Esquivar"]


def main():
    """Main function that creates the test data in the database."""

    puser = input(f"Postgres user ({POSTGRES_USER}): ") or POSTGRES_USER
    ppass = input(
        f"Postgres password ({POSTGRES_PASSWORD}): ") or POSTGRES_PASSWORD
    phost = input(f"Postgres host ({POSTGRES_HOST}): ") or POSTGRES_HOST
    engine_path = f"postgresql://{puser}:{ppass}@{phost}:5432"
    engine = create_engine(engine_path)

    Base.metadata.create_all(engine)
    now = datetime.now(timezone.utc)

    with Session(engine) as session:
        # Get first user
        user = session.query(User).first()
        if not user:
            # Create a user
            user = User(
                email="test@test.com",
                hashed_password=get_password_hash("123456"),
                verified=True,
                verification_code="123456",
                name="Test Testo Test",
                year_of_birth=1995
            )
            session.add(user)

        print(f"Using user: {user.email}")

        # Add some shapes to the user
        session.add(Shape(
            date=now - timedelta(days=7),
            weight=80,
            height=180,
            user_email=user.email
        ))

        session.add(Shape(
            date=now - timedelta(days=3),
            weight=77,
            height=180,
            user_email=user.email
        ))

        session.add(Shape(
            date=now,
            weight=70,
            height=180,
            user_email=user.email
        ))

        # Add some activities to the user
        for _ in range(ACTIVITY_COUNT):
            date = now - \
                timedelta(days=random.randint(0, ACTIVITIES_FROM),
                          hours=random.randint(-12, 12))
            while date > now:
                date = now - \
                    timedelta(days=random.randint(0, ACTIVITIES_FROM),
                              hours=random.randint(-12, 12))

            session.add(Activity(
                date=date,
                user_email=user.email,
                duration=random.randint(60, 120),
                score=random.randint(1, 20),
                activity_points=random.randint(100, 200),
                minigame=random.choice(MINIGAMES),
                extra_data="{}"
            ))

        session.commit()


if __name__ == "__main__":
    main()
