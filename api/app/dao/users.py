from core.db import engine
from sqlalchemy.orm import Session

from models.users import User as UserModel
from schemas.users import User as UserSchema

class UserDAO:
    @staticmethod
    def get_user(email: str) -> UserSchema | None:
        with Session(engine) as session:
            user = session.query(UserModel).filter(UserModel.email == email).first()
            if user:
                return UserSchema(
                    email=user.email,
                    verified=user.verified,
                    verification_code=user.verification_code,
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

