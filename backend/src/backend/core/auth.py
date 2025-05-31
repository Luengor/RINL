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
    user = AuthDAO.get_user_email(email, session)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user


async def get_current_user_auth(token: Annotated[str, Depends(oauth2_scheme)], session: Session = Depends(get_db)) -> UserAuth:
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
    user_auth = await get_current_user_auth(token, session)
    user = UserDAO.get_user(user_auth.email, session)
    if user is None:
        raise NOT_FOUND

    return user


async def get_current_verified_user(user: UserBase = Depends(get_current_user)) -> UserBase:
    if not user.verified:
        raise NOT_VERIFIED
    return user
