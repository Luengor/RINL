"""Authentication dependencies and utilities."""

from typing import Annotated

from jwt.exceptions import InvalidTokenError
from fastapi import Depends, HTTPException
from sqlalchemy.orm import Session

from backend.schemas.auth import UserAuth
from backend.schemas.users import UserBase
from backend.dao.auth import AuthDAO
from backend.dao.user import UserDAO
from backend.core.auth_utils import verify_password, oauth2_scheme, decode_token
from backend.core.db import get_db

# CHANGING THIS STATUS CODES WILL BREAK THE FRONTEND
INVALID_TOKEN = HTTPException(status_code=401, detail="Invalid token")
NOT_VERIFIED = HTTPException(status_code=400, detail="User not verified")
NOT_FOUND = HTTPException(status_code=404, detail="User not found")

# Authentication


def authenticate_user(email: str, password: str, session: Session) -> UserAuth | None:
    """Authenticates a user by their email and password.

    It searches for the user in the database using the provided email,
    verifies the password, and returns the user if authentication is successful.

    Args:
        email (str): The email of the user to authenticate.
        password (str): The password of the user to authenticate.
        session (Session): The database session to use for querying.

    Returns:
        UserAuth | None: The authenticated user if successful, otherwise None.
    """
    user = AuthDAO.get_user_email(email.lower(), session)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user


async def get_current_user_auth(token: Annotated[str, Depends(oauth2_scheme)], session: Session = Depends(get_db)) -> UserAuth:
    """Retrieves the current authenticated user based on the provided token.

    Args:
        token (str): The JWT token used for authentication.
        session (Session): The database session to use for querying.

    Returns:
        UserAuth: The authenticated user object.

    Raises:
        HTTPException: If the token is invalid or the user is not found.
    """
    try:
        payload = decode_token(token)
        uuid = payload.get("sub")
        if uuid is None:
            raise INVALID_TOKEN
    except InvalidTokenError as e:
        print(f"Invalid token: {e}", flush=True)
        raise INVALID_TOKEN

    user = AuthDAO.get_user(int(uuid), session)
    if user is None:
        raise NOT_FOUND
    return user


async def get_current_user(token: Annotated[str, Depends(oauth2_scheme)], session: Session = Depends(get_db)) -> UserBase:
    """Retrieves the current user based on the provided token.

    Args:
        token (str): The JWT token used for authentication.
        session (Session): The database session to use for querying.

    Returns:
        UserBase: The current user object.

    Raises:
        HTTPException: If the token is invalid or the user is not found.
    """
    user_auth = await get_current_user_auth(token, session)
    user = UserDAO.get_user(user_auth.email.lower(), session)
    if user is None:
        raise NOT_FOUND

    return user


async def get_current_verified_user(user: UserBase = Depends(get_current_user)) -> UserBase:
    """Retrieves the current verified user.

    Args:
        user (UserBase): The current user object.

    Returns:
        UserBase: The user object if it is verified.

    Raises:
        HTTPException: If the user is not verified.
    """
    if not user.verified:
        raise NOT_VERIFIED
    return user
