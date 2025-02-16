from fastapi import APIRouter, Depends, Response

from core.auth import get_current_user
from dao.users import UserDAO
from schemas.users import User

router = APIRouter(prefix="/users")

@router.get("/me", response_model=User)
async def get_me(user: User = Depends(get_current_user)) -> User:
    return user

@router.post("/verify/{verification_code}")
async def verify_user(verification_code: str, user: User = Depends(get_current_user)):
    if user.verified:
        return Response(status_code=400, content="User already verified")
    
    elif UserDAO.verify_user(user.email, verification_code):
        return Response(status_code=200, content="User verified")
    
    return Response(status_code=400, content="Invalid verification code")

