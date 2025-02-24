from core.db import engine 
from sqlalchemy.orm import Session

from models.users import User
from schemas.auth import UserAuth

class AuthDAO:
    @staticmethod
    def get_user(email: str) -> UserAuth | None:
        with Session(engine) as session:
            user = session.query(User).filter(User.email == email).first()
            if user:
                return UserAuth(email=user.email, hashed_password=user.hashed_password)

        return None

