"""Data Access Object for handling user authentication in the database."""

from sqlalchemy.orm import Session

from backend.models.user import User
from backend.schemas.auth import UserAuth


class AuthDAO:
    """Data Access Object for handling user authentication in the database."""

    @staticmethod
    def get_user(uuid: int, session: Session) -> UserAuth | None:
        """Retrieve a user by UUID from the database.

        Args:
            uuid (int): The UUID of the user to retrieve.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            UserAuth | None: An instance of UserAuth if the user exists, otherwise None.
        """
        user = session.query(User).filter(User.uuid == uuid).first()
        if user:
            return UserAuth(uuid=uuid, email=user.email, hashed_password=user.hashed_password)

        return None

    @staticmethod
    def get_user_email(email: str, session: Session) -> UserAuth | None:
        """Retrieve a user authentication by email from the database.

        Args:
            email (str): The email of the user to retrieve.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            UserAuth | None: An instance of UserAuth if the user exists, otherwise None.
        """
        user = session.query(User).filter(User.email == email).first()
        if user:
            return UserAuth(uuid=user.uuid, email=user.email, hashed_password=user.hashed_password)

        return None
