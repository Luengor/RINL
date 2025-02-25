from fastapi import APIRouter, Depends, Response, HTTPException

from core.auth import get_current_user
from dao.user import UserDAO
from schemas.users import UserBase, RegisterUser 

router = APIRouter(
    prefix="/user",
    tags=["user"],
)

@router.post("/", response_model=UserBase)
async def create_user(user: RegisterUser):
    if (new_user := UserDAO.create_user(user)):
        return new_user

    print(new_user)
    raise HTTPException(status_code=400, detail="User already exists")

@router.post("/verify/{verification_code}")
async def verify_user(verification_code: str, user: UserBase = Depends(get_current_user)):
    if user.verified:
        return Response(status_code=400, content="User already verified")
    
    elif UserDAO.verify_user(user.email, verification_code):
        return Response(status_code=200, content="User verified")
    
    return Response(status_code=400, content="Invalid verification code")

@router.get("/me", response_model=UserBase)
async def get_me(user: UserBase = Depends(get_current_user)):
    return user

@router.delete("/me")
async def delete_me(user: UserBase = Depends(get_current_user)):
    UserDAO.delete_user(user.email)
    response = Response(status_code=200, content="User deleted")
    response.delete_cookie("access_token")
    return response 
