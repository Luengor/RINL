from fastapi import APIRouter, Depends, Response, HTTPException
from sqlalchemy.orm import Session
from backend.core.auth import get_current_user, get_current_verified_user
from backend.core.db import get_db
from backend.core.mail import get_send_email, SendEmailType
from backend.dao.user import UserDAO
from backend.schemas.users import UserBase, RegisterUser, ModifyUser

router = APIRouter(
    prefix="/user",
    tags=["user"],
)


@router.post("/", response_model=UserBase)
async def create_user(
    user: RegisterUser, session: Session = Depends(get_db), send_email: SendEmailType = Depends(get_send_email)
):
    return UserDAO.create_user(user, session, send_email)


@router.post("/verify-email")
async def send_verification_email(
    user: UserBase = Depends(get_current_user),
    session: Session = Depends(get_db),
    send_email: SendEmailType = Depends(get_send_email),
):
    if user.verified:
        raise HTTPException(
            status_code=400, detail="User already verified"
        )

    UserDAO.send_verification_code(user.email, session, send_email)
    return Response(status_code=200, content="Verification email sent")


@router.post("/verify/{verification_code}")
async def verify_user(
    verification_code: str,
    user: UserBase = Depends(get_current_user),
    session: Session = Depends(get_db),
):
    if user.verified:
        raise HTTPException(
            status_code=400, detail="User already verified"
        )

    if UserDAO.verify_user(user.email, verification_code, session):
        return Response(status_code=200, content="User verified")

    raise HTTPException(
        status_code=400, detail="Invalid verification code"
    )


@router.get("/me", response_model=UserBase)
async def get_me(user: UserBase = Depends(get_current_user)):
    return user


@router.put("/me", response_model=UserBase)
async def update_me(
    modifications: ModifyUser,
    user: UserBase = Depends(get_current_verified_user),
    session: Session = Depends(get_db),
    send_email: SendEmailType = Depends(get_send_email),
):
    return UserDAO.update_user(user, modifications, session, send_email)


@router.delete("/me")
async def delete_me(
    user: UserBase = Depends(get_current_user), session: Session = Depends(get_db)
):
    UserDAO.delete_user(user.email, session)
    response = Response(status_code=200, content="User deleted")
    response.delete_cookie("access_token")
    return response
