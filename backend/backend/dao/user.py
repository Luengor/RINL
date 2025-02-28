from random import choices
from fastapi import HTTPException

from core.auth_utils import get_password_hash
from core.mail import send_email
from sqlalchemy.orm import Session

from models.user import User as UserModel
from schemas.users import UserBase as UserSchema
from schemas.users import RegisterUser as RegisterUserSchema 

def create_verification_code() -> str:
    return "".join(choices("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", k=6))

class UserDAO:
    @staticmethod
    def create_user(user: RegisterUserSchema, session: Session) -> UserSchema:
        # Check if user already exists
        if UserDAO.get_user(user.email, session):
            raise HTTPException(status_code=400, detail="User already exists")

        # Send verification email
        verification_code = create_verification_code()
        if not send_email(
                email=user.email,
                subject="Verify your email",
                content=f"Your verification code is {verification_code}"):
            raise HTTPException(status_code=500, detail="Failed to send verification email")

        # Create user
        user_model = UserModel(
            email=user.email,
            hashed_password=get_password_hash(user.password),
            name=user.name,
            year_of_birth=user.year_of_birth,
            verification_code=verification_code)
        
        session.add(user_model)
        session.commit()
        user_schema = UserSchema.model_validate(user_model)
        return user_schema

    @staticmethod
    def get_user(email: str, session: Session) -> UserSchema | None:
        user = session.query(UserModel).filter(UserModel.email == email).first()
        if user:
            return UserSchema.model_validate(user)

        return None
    
    @staticmethod
    def verify_user(email: str, verification_code: str, session: Session) -> bool:
        # Check the user exists
        if not UserDAO.get_user(email, session):
            return False

        # Verify user
        user = session.query(UserModel).filter(UserModel.email == email).first()
        if user and user.verification_code == verification_code:
            user.verified = True
            session.commit()
            return True

        return False

    @staticmethod
    def delete_user(email: str, session: Session):
        # Check the user exists
        if not UserDAO.get_user(email, session):
            return
        
        # Delete user
        session.delete(session.query(UserModel).filter(UserModel.email == email).first())
        session.commit()
