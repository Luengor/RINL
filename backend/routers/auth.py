from typing import Annotated
from datetime import timedelta

from fastapi import APIRouter, Depends, HTTPException, status, Response
from fastapi.security import OAuth2PasswordRequestForm

from schemas.auth import Token
from core.auth import authenticate_user, oauth2_scheme
from core.auth_utils import create_access_token, get_expire_time

ACCESS_TOKEN_EXPIRE_MINUTES = 60

router = APIRouter(
    prefix="/token",
)

@router.post("/", response_model=Token)
async def login_for_token(
    form_data: Annotated[OAuth2PasswordRequestForm, Depends()],
    response: Response
) -> Token:
    user = authenticate_user(form_data.username, form_data.password) 
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"},
        )
    
    access_token_expires = timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": user.email}, expires_delta=access_token_expires
    )

    response.set_cookie(
        key="session",
        value=access_token,
        httponly=True,
        secure=True,
        expires=get_expire_time(access_token_expires),
    )
    return Token(access_token=access_token, token_type="bearer")

@router.post("/verify")
async def verify_token(token: str = Depends(oauth2_scheme)):
    return {"token": token}

