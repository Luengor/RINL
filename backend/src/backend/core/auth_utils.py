from typing import Any
from datetime import timedelta, datetime, timezone
from os import environ

import jwt
from fastapi.security import OAuth2PasswordBearer
from passlib.context import CryptContext

# Some constants
SECRET_KEY = environ["JWT_SECRET"]
ALGORITHM = "HS256"

## Dependencies
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")


# Passwords
def verify_password(plain_password: str, hashed_password: str) -> bool:
    return pwd_context.verify(plain_password, hashed_password)


def get_password_hash(password: str) -> str:
    return pwd_context.hash(password)


# Tokens
def get_expire_time(expires_delta: timedelta | None = None) -> datetime:
    return (
        datetime.now(timezone.utc) + expires_delta
        if expires_delta
        else datetime.now(timezone.utc)
    )


def create_access_token(
    data: dict[str, Any], expires_delta: timedelta | None = None
) -> str:
    to_encode = data.copy()
    expire = (
        get_expire_time(expires_delta)
        if expires_delta
        else get_expire_time(timedelta(minutes=15))
    )
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt


def decode_token(token: str) -> dict[str, Any]:
    return jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
