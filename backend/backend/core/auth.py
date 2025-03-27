from typing import Annotated

from jwt.exceptions import InvalidTokenError 
from fastapi import Depends, HTTPException

from schemas.auth import UserAuth
from schemas.users import UserBase
from dao.auth import AuthDAO
from dao.user import UserDAO 
from core.auth_utils import verify_password, oauth2_scheme, decode_token
from core.db import get_db

# CHANGING THIS STATUS CODES WILL BREAK THE FRONTEND
INVALID_TOKEN = HTTPException(status_code=401, detail="Invalid token")
NOT_VERIFIED = HTTPException(status_code=400, detail="User not verified")
NOT_FOUND = HTTPException(status_code=404, detail="User not found")

# Authentication
def authenticate_user(email: str, password: str, session) -> UserAuth | None:
    user = AuthDAO.get_user(email, session)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user

async def get_current_user_auth(token: Annotated[str, Depends(oauth2_scheme)], session = Depends(get_db)) -> UserAuth:
    try:
        payload = decode_token(token)
        email:str = payload.get("sub")  # type: ignore
        if email is None:
            raise INVALID_TOKEN 
    except InvalidTokenError:
        raise INVALID_TOKEN 

    user = AuthDAO.get_user(email, session)
    if user is None:
        raise NOT_FOUND 
    return user

async def get_current_user(token: Annotated[str, Depends(oauth2_scheme)], session = Depends(get_db)) -> UserBase:
    try:
        payload = decode_token(token)
        email:str = payload.get("sub")  # type: ignore
        if email is None:
            raise INVALID_TOKEN 
    except InvalidTokenError:
        raise INVALID_TOKEN 

    user = UserDAO.get_user(email, session)
    if user is None:
        raise NOT_FOUND 
    return user

async def get_current_verified_user(user: UserBase = Depends(get_current_user)) -> UserBase:
    if not user.verified:
        raise NOT_VERIFIED 
    return user
