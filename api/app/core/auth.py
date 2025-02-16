from datetime import timedelta, datetime, timezone
from typing import Annotated
from os import environ

import jwt
from jwt.exceptions import InvalidTokenError 
from fastapi import Depends
from fastapi.security import OAuth2PasswordBearer
from passlib.context import CryptContext

from schemas.auth import UserAuth
from schemas.users import User
from dao.auth import AuthDAO
from dao.users import UserDAO 

# Some constants
SECRET_KEY = environ["JWT_SECRET"]
ALGORITHM = "HS256"

## Dependencies
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# Passwords
def verify_password(plain_password: str, hashed_password: str) -> bool:
    return pwd_context.verify(plain_password, hashed_password)

def get_password_hash(password: str) -> str:
    return pwd_context.hash(password)

# Tokens
def create_access_token(data: dict, expires_delta: timedelta | None = None) -> str:
    to_encode = data.copy()
    if expires_delta:
        expire = datetime.now(timezone.utc) + expires_delta
    else:
        expire = datetime.now(timezone.utc) + timedelta(minutes=15)
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt

def decode_token(token: str) -> dict:
    return jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])

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

async def get_current_user(token: Annotated[str, Depends(oauth2_scheme)]) -> User:
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
