from sqlalchemy.orm import Session

from backend.models.user import User
from backend.schemas.auth import UserAuth

class AuthDAO:
    @staticmethod
    def get_user(email: str, session: Session) -> UserAuth | None:
        user = session.query(User).filter(User.email == email).first()
        if user:
            return UserAuth(email=user.email, hashed_password=user.hashed_password)

        return None

