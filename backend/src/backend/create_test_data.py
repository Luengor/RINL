from os import environ
from sqlalchemy import create_engine
from sqlalchemy.orm import Session

from backend.core.auth_utils import get_password_hash
from backend.core.db import Base
from backend.models.user import User
from backend.models.shape import Shape
from backend.models.activity import Activity

from datetime import datetime, timedelta, timezone

import random

ENGINE_PATH = f"postgresql://{environ.get('POSTGRES_USER')}:{environ.get('POSTGRES_PASSWORD')}@{environ.get('POSTGRES_HOST')}:5432"
engine = create_engine(ENGINE_PATH)

ACTIVITY_COUNT = 2000
ACTIVITIES_FROM = 100
MINIGAMES = ["test1", "test2"]


def main():
    Base.metadata.create_all(engine)
    now = datetime.now(timezone.utc)

    with Session(engine) as session:
        # Create test user
        user = User(
            email="test@test.com",
            hashed_password=get_password_hash("123456"),
            verified=True,
            verification_code="123456",
            name="Test Testo Test",
            year_of_birth=1995
        )
        session.add(user)

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
        for i in range(ACTIVITY_COUNT):
            date = now - \
                timedelta(days=random.randint(0, ACTIVITIES_FROM),
                          hours=random.randint(-12, 12))
            while (date > now):
                date = now - \
                    timedelta(days=random.randint(0, ACTIVITIES_FROM),
                              hours=random.randint(-12, 12))

            session.add(Activity(
                date=date,
                user_email="test@test.com",
                duration=random.randint(60, 120),
                score=random.randint(1, 20),
                activity_points=random.randint(100, 200),
                minigame=random.choice(MINIGAMES),
                extra_data="{}"
            ))

        session.commit()


if __name__ == "__main__":
    main()
