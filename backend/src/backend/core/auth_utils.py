"""Utility functions for authentication and token management."""

from typing import Any
from datetime import timedelta, datetime, timezone
import os

import jwt
from fastapi.security import OAuth2PasswordBearer
from passlib.context import CryptContext

# Some constants
SECRET_KEY = os.environ.get("JWT_SECRET", "very_secret_key")
"""Secret key for JWT encoding and decoding. Obtained from the environment variable 'JWT_SECRET'."""

ALGORITHM = "HS256"
"""Algorithm used for JWT encoding and decoding."""

# Dependencies
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")
"""Dependency for OAuth2 password bearer token authentication."""

pwd_context = CryptContext(schemes=["argon2"], deprecated="auto", argon2__type="ID",
                           argon2__time_cost=2, argon2__memory_cost=19456, argon2__parallelism=1)
"""Password context for hashing and verifying passwords using argon2."""


# Passwords
def verify_password(plain_password: str, hashed_password: str) -> bool:
    """Verify a plain password agains a hashed password.

    Args:
        plain_password (str): The plain text password to verify.
        hashed_password (str): The hashed password to compare against.

    Returns:
        bool: True if the plain password matches the hashed password, False otherwise.
    """
    return pwd_context.verify(plain_password, hashed_password)


def get_password_hash(password: str) -> str:
    """Hash a password using argon2.

    Args:
        password (str): The plain text password to hash.

    Returns:
        str: The hashed password.
    """
    return pwd_context.hash(password)


# Tokens
def get_expire_time(expires_delta: timedelta = timedelta(minutes=15)) -> datetime:
    """Get the expiration time for a token.

    Args:
        expires_delta (timedelta | None): The time delta for expiration. Defaults to 15 minutes.

    Returns:
        datetime: The expiration time as a UTC datetime object.
    """
    return datetime.now(timezone.utc) + expires_delta


def create_access_token(
    data: dict[str, Any], expires_delta: timedelta | None = None
) -> str:
    """Create a JWT access token.

    This function encodes the given data into a JWT token with an expiration time.

    Args:
        data (dict[str, Any]): The data to encode in the token.
        expires_delta (timedelta | None): The time delta for expiration. Defaults to 15 minutes.
    """
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
    """Decode a JWT token.

    Args:
        token (str): The JWT token to decode.

    Returns:
        dict[str, Any]: The decoded token data.
    """
    token_dict = jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
    if 'sub' not in token_dict:
        raise jwt.InvalidTokenError("Token does not contain 'sub' field")

    return token_dict
