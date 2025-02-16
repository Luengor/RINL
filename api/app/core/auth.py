from datetime import timedelta, datetime, timezone
from typing import Annotated

import jwt
from jwt.exceptions import InvalidTokenError 
from fastapi import Depends
from fastapi.security import OAuth2PasswordBearer
from passlib.context import CryptContext

from schemas.auth import Token, UserAuth
from dao.auth import AuthDAO

# Some constants
SECRET_KEY = "b8c535ae0987b4c49278ffb46ef76f4d3eddc380a6019e89ca3e74e61d42f9ab"
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

async def get_current_user(token: Annotated[str, Depends(oauth2_scheme)]) -> UserAuth:
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
