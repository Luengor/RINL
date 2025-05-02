from random import choices
from fastapi import HTTPException
from typing import Callable

from backend.core.auth_utils import get_password_hash
from sqlalchemy.orm import Session

from backend.models.user import User as UserModel
from backend.schemas.users import UserBase as UserSchema
from backend.schemas.users import ModifyUser as ModifyUserSchema
from backend.schemas.users import RegisterUser as RegisterUserSchema 

def create_verification_code() -> str:
    return "".join(choices("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", k=6))

class UserDAO:
    @staticmethod
    def create_user(user: RegisterUserSchema, session: Session, send_email: Callable[[str, str, str], bool]) -> UserSchema:
        # Check if user already exists
        if UserDAO.get_user(user.email, session):
            raise HTTPException(status_code=400, detail="User already exists")

        # Create user
        user_model = UserModel(
            email=user.email,
            hashed_password=get_password_hash(user.password),
            name=user.name,
            year_of_birth=user.year_of_birth,
            verification_code='000000')

        session.add(user_model)
        session.commit()

        # Update verification code
        UserDAO.send_verification_code(user.email, session, send_email)
        
        # Return user schema
        user_schema = UserSchema.model_validate(user_model)
        return user_schema
    
    @staticmethod
    def send_verification_code(user_email: str, session: Session, send_email: Callable[[str, str, str], bool]):
        # Send verification email
        verification_code = create_verification_code()
        if not send_email(
                user_email,
                "Verify your email",
                f"Your verification code is {verification_code}"):
            raise HTTPException(status_code=500, detail="Failed to send verification email")
        
        # Update user
        user_model = session.query(UserModel).filter(UserModel.email == user_email).first()
        if not user_model:
            raise HTTPException(status_code=400, detail="User does not exist")
        
        user_model.verification_code = verification_code
        
        session.commit()

    @staticmethod
    def update_user(base_user: UserSchema, modify: ModifyUserSchema, session: Session, send_email: Callable[[str, str, str], bool]) -> UserSchema:
        # Check the user exists
        if not UserDAO.get_user(base_user.email, session):
            raise HTTPException(status_code=400, detail="User does not exist")
        
        # Check if email is already taken
        if modify.email and modify.email != base_user.email and UserDAO.get_user(modify.email, session):
            raise HTTPException(status_code=400, detail="Email already taken")

        # Update user
        user = session.query(UserModel).filter(UserModel.email == base_user.email).first()
        assert user is not None
        user.name = modify.name or user.name
        user.year_of_birth = modify.year_of_birth or user.year_of_birth

        if modify.email:
            user.email = modify.email
            user.verified = False
            user.verification_code = create_verification_code()
            if not send_email(
                    user.email,
                    "Verify your email",
                    f"Your verification code is {user.verification_code}"):
                raise HTTPException(status_code=500, detail="Failed to send verification email")
        
        session.commit()

        return UserSchema.model_validate(user)

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
