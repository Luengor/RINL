from typing import Annotated

from jwt.exceptions import InvalidTokenError 
from fastapi import Depends

from schemas.auth import UserAuth
from schemas.users import UserBase
from dao.auth import AuthDAO
from dao.user import UserDAO 
from core.auth_utils import verify_password, oauth2_scheme, decode_token

# Authentication
def authenticate_user(email: str, password: str) -> UserAuth | None:
    user = AuthDAO.get_user(email)
    if not user:
        return None
    if not verify_password(password, user.hashed_password):
        return None
    return user

async def get_current_user_auth(token: Annotated[str, Depends(oauth2_scheme)]) -> UserAuth:
    try:
        payload = decode_token(token)
        email:str = payload.get("sub")  # type: ignore
        if email is None:
            raise Exception("Invalid token")
    except InvalidTokenError:
        raise Exception("Invalid token")

    user = AuthDAO.get_user(email)
    if user is None:
        raise Exception("User not found")
    return user

# TODO: better exceptions
async def get_current_user(token: Annotated[str, Depends(oauth2_scheme)]) -> UserBase:
    try:
        payload = decode_token(token)
        email:str = payload.get("sub")  # type: ignore
        if email is None:
            raise Exception("Invalid token")
    except InvalidTokenError:
        raise Exception("Invalid token")

    user = UserDAO.get_user(email)
    if user is None:
        raise Exception("User not found")
    return user

async def get_current_verified_user(user: UserBase = Depends(get_current_user)) -> UserBase:
    if not user.verified:
        raise Exception("User not verified")
    return user
