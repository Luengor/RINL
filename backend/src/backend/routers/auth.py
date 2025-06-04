"""Auth API router for handling user login and token generation."""

from typing import Annotated
from datetime import timedelta
from fastapi import APIRouter, Depends, HTTPException, status, Response
from fastapi.security import OAuth2PasswordRequestForm
from sqlalchemy.orm import Session
from backend.schemas.auth import Token
from backend.core.auth import authenticate_user, oauth2_scheme
from backend.core.db import get_db
from backend.core.auth_utils import create_access_token, get_expire_time

ACCESS_TOKEN_EXPIRE_MINUTES = 180

router = APIRouter(
    prefix="/login",
    tags=["login"],
)


@router.post("/", response_model=Token)
async def login_for_token(
    form_data: Annotated[OAuth2PasswordRequestForm, Depends()],
    response: Response,
    session: Session = Depends(get_db)
) -> Token:
    """Login endpoint to authenticate user and generate access token.

    This function is an endpoint.

    Args:
        form_data (OAuth2PasswordRequestForm): The form data containing username and password, automatically injected by FastAPI.
        response (Response): The HTTP response object.
        session (Session): The database session, automatically injected by FastAPI.
    """
    user = authenticate_user(form_data.username, form_data.password, session)
    if not user:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect email or password",
            headers={"WWW-Authenticate": "Bearer"},
        )

    access_token_expires = timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    access_token = create_access_token(
        data={"sub": str(user.uuid)}, expires_delta=access_token_expires
    )

    response.set_cookie(
        key="access_token",
        value=access_token,
        httponly=True,
        secure=True,
        expires=get_expire_time(access_token_expires),
    )
    return Token(access_token=access_token, token_type="bearer")


@router.get("/", response_model=dict)
async def check_token(token: Annotated[str, Depends(oauth2_scheme)]) -> dict[str, str]:
    """Check if the provided token is valid.

    This function is an endpoint.

    Args:
        token (str): The access token to check, automatically injected by FastAPI.
    Returns:
        dict[str, str]: A dictionary containing the token if valid.
    """
    return {"token": token}
