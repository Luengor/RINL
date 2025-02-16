from random import choices

from core.auth_utils import get_password_hash
from core.db import engine
from sqlalchemy.orm import Session

from models.users import User as UserModel
from schemas.users import UserBase as UserSchema
from schemas.users import RegisterUser as RegisterUserSchema 

def create_verification_code() -> str:
    return "".join(choices("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", k=6))

class UserDAO:
    @staticmethod
    def create_user(user: RegisterUserSchema) -> UserSchema | None:
        # Check if user already exists
        if UserDAO.get_user(user.email):
            return None

        # Create user
        with Session(engine) as session:
            user_model = UserModel(
                email=user.email,
                hashed_password=get_password_hash(user.password),
                name=user.name,
                year_of_birth=user.year_of_birth,
                verification_code=create_verification_code())
            
            session.add(user_model)
            session.commit()

            return UserSchema(
                email=user_model.email,
                verified=user_model.verified,
                name=user_model.name,
                year_of_birth=user_model.year_of_birth)

    @staticmethod
    def get_user(email: str) -> UserSchema | None:
        with Session(engine) as session:
            user = session.query(UserModel).filter(UserModel.email == email).first()
            if user:
                return UserSchema(
                    email=user.email,
                    verified=user.verified,
                    name=user.name,
                    year_of_birth=user.year_of_birth)
        return None
    
    @staticmethod
    def verify_user(email: str, verification_code: str) -> bool:
        with Session(engine) as session:
            user = session.query(UserModel).filter(UserModel.email == email).first()
            if user and user.verification_code == verification_code:
                user.verified = True
                session.commit()
                return True
        return False

