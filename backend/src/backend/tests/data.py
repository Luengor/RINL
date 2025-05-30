from backend.schemas.users import UserFull

test_user = UserFull(
    email="test",
    verified=True,
    name="test",
    year_of_birth=2000,
    password="test",
    verification_code="123456",
)

test_unverified_user = UserFull(
    email="test2",
    verified=False,
    name="test",
    year_of_birth=2000,
    password="test",
    verification_code="123456",
)
