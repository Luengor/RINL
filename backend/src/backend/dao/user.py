"""Data Access Object for handling user-related operations in the database."""

from fastapi import HTTPException
from sqlalchemy.orm import Session
from backend.core.auth_utils import get_password_hash
from backend.models.user import User as UserModel
from backend.schemas.users import UserBase as UserSchema
from backend.schemas.users import ModifyUser as ModifyUserSchema
from backend.schemas.users import RegisterUser as RegisterUserSchema


class UserDAO:
    """Data Access Object for handling user-related operations in the database."""

    @staticmethod
    def create_user(user: RegisterUserSchema, session: Session) -> UserSchema:
        """Create a new user in the database.

        Args:
            user (RegisterUserSchema): The user data to create.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            UserSchema: The created user schema.

        Raises:
            HTTPException: If the user already exists or if there is an error sending the verification email.
        """
        # Check if user already exists
        if UserDAO.get_user(user.email, session):
            raise HTTPException(status_code=400, detail="User already exists")

        # Create user
        user_model = UserModel(
            email=user.email,
            hashed_password=get_password_hash(user.password),
            name=user.name,
            year_of_birth=user.year_of_birth)

        session.add(user_model)
        session.commit()

        # Return user schema
        user_schema = UserSchema.model_validate(user_model)
        return user_schema

    @staticmethod
    def update_user(base_user: UserSchema, modify: ModifyUserSchema, session: Session) -> UserSchema:
        """Update an existing user in the database.

        Args:
            base_user (UserSchema): The current user data.
            modify (ModifyUserSchema): The modifications to apply to the user.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            UserSchema: The updated user schema.

        Raises:
            HTTPException: If the user does not exist, if the email is already taken, or if there is an error sending the verification email.
        """
        # Check the user exists
        if not UserDAO.get_user(base_user.email, session):
            raise HTTPException(status_code=400, detail="User does not exist")

        # Check if email is already taken
        if modify.email and modify.email != base_user.email and UserDAO.get_user(modify.email, session):
            raise HTTPException(status_code=400, detail="Email already taken")

        # Update user
        user = session.query(UserModel).filter(
            UserModel.email == base_user.email).first()
        assert user is not None
        user.name = modify.name or user.name
        user.email = modify.email or user.email
        user.year_of_birth = modify.year_of_birth or user.year_of_birth

        session.commit()

        return UserSchema.model_validate(user)

    @staticmethod
    def get_user(email: str, session: Session) -> UserSchema | None:
        """Retrieve a user by email from the database.

        Args:
            email (str): The email of the user to retrieve.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            UserSchema | None: An instance of UserSchema if the user exists, otherwise None.
        """
        user = session.query(UserModel).filter(
            UserModel.email == email).first()

        if user:
            return UserSchema.model_validate(user)

        return None

    @staticmethod
    def verify_user(email: str, session: Session) -> bool:
        """Verify a user's email address.

        Args:
            email (str): The email of the user to verify.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            bool: True if the user is successfully verified, False otherwise.
        """
        # Verify user
        user = session.query(UserModel).filter(
            UserModel.email == email).first()

        if user is not None:
            user.verified = True
            session.commit()
            return True

        return False

    @staticmethod
    def delete_user(email: str, session: Session):
        """Delete a user from the database.

        Args:
            email (str): The email of the user to delete.
            session (Session): The SQLAlchemy session to use for the database operations.

        Raises:
            HTTPException: If the user does not exist.
        """
        # Check the user exists
        if not UserDAO.get_user(email, session):
            return

        # Delete user
        session.delete(session.query(UserModel).filter(
            UserModel.email == email).first())
        session.commit()
