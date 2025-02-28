from fastapi import APIRouter, Depends, Response, HTTPException

from core.auth import get_current_user
from core.db import get_db
from dao.user import UserDAO
from schemas.users import UserBase, RegisterUser 

router = APIRouter(
    prefix="/user",
    tags=["user"],
)

@router.post("/", response_model=UserBase)
async def create_user(user: RegisterUser, session=Depends(get_db)):
    return UserDAO.create_user(user, session)

@router.post("/verify/{verification_code}")
async def verify_user(verification_code: str, user: UserBase = Depends(get_current_user), session=Depends(get_db)):
    if user.verified:
        return Response(status_code=400, content="User already verified")
    
    elif UserDAO.verify_user(user.email, verification_code, session):
        return Response(status_code=200, content="User verified")
    
    return Response(status_code=400, content="Invalid verification code")

@router.get("/me", response_model=UserBase)
async def get_me(user: UserBase = Depends(get_current_user)):
    return user

@router.delete("/me")
async def delete_me(user: UserBase = Depends(get_current_user), session=Depends(get_db)):
    UserDAO.delete_user(user.email, session)
    response = Response(status_code=200, content="User deleted")
    response.delete_cookie("access_token")
    return response 
