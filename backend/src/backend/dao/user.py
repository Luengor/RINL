from random import choices
from fastapi import HTTPException
from sqlalchemy.orm import Session
from backend.core.auth_utils import get_password_hash
from backend.core.mail import SendEmailType
from backend.models.user import User as UserModel
from backend.schemas.users import UserBase as UserSchema
from backend.schemas.users import ModifyUser as ModifyUserSchema
from backend.schemas.users import RegisterUser as RegisterUserSchema


def create_verification_code(length: int = 6) -> str:
    """Generate a random verification code consisting alphanumeric characters.

    Args:
        length (int): The length of the verification code. Defaults to 6.

    Returns:
        str: The generated verification code. 
    """
    return "".join(choices("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", k=length))


class UserDAO:
    """Data Access Object for handling user-related operations in the database."""

    @staticmethod
    def create_user(user: RegisterUserSchema, session: Session, send_email: SendEmailType) -> UserSchema:
        """Create a new user in the database.

        Args:
            user (RegisterUserSchema): The user data to create.
            session (Session): The SQLAlchemy session to use for the database operations.
            send_email (SendEmailType): Function to send verification email.

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
    def send_verification_code(user_email: str, session: Session, send_email: SendEmailType):
        """Send a verification code to the user's email.

        Args:
            user_email (str): The email of the user to send the verification code to.
            session (Session): The SQLAlchemy session to use for the database operations.
            send_email (SendEmailType): Function to send the verification email.

        Raises:
            HTTPException: If the user does not exist or if there is an error sending the email.
        """
        # Send verification email
        verification_code = create_verification_code()
        if not send_email(
                user_email,
                "Verify your email",
                f"Your verification code is {verification_code}"):
            raise HTTPException(
                status_code=500, detail="Failed to send verification email")

        # Update user
        user_model = session.query(UserModel).filter(
            UserModel.email == user_email).first()
        if not user_model:
            raise HTTPException(status_code=400, detail="User does not exist")

        user_model.verification_code = verification_code

        session.commit()

    @staticmethod
    def update_user(base_user: UserSchema, modify: ModifyUserSchema, session: Session, send_email: SendEmailType) -> UserSchema:
        """Update an existing user in the database.

        Args:
            base_user (UserSchema): The current user data.
            modify (ModifyUserSchema): The modifications to apply to the user.
            session (Session): The SQLAlchemy session to use for the database operations.
            send_email (SendEmailType): Function to send verification email if email is changed.

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
        user.year_of_birth = modify.year_of_birth or user.year_of_birth

        if modify.email:
            user.email = modify.email
            user.verified = False
            user.verification_code = create_verification_code()
            if not send_email(
                    user.email,
                    "Verify your email",
                    f"Your verification code is {user.verification_code}"):
                raise HTTPException(
                    status_code=500, detail="Failed to send verification email")

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
    def verify_user(email: str, verification_code: str, session: Session) -> bool:
        """Verify a user's email using the verification code.

        Args:
            email (str): The email of the user to verify.
            verification_code (str): The verification code sent to the user's email.
            session (Session): The SQLAlchemy session to use for the database operations.

        Returns:
            bool: True if the user is successfully verified, False otherwise.

        Raises:
            HTTPException: If the user does not exist or if the verification code is invalid.
        """
        # Check the user exists
        if not UserDAO.get_user(email, session):
            return False

        # Verify user
        user = session.query(UserModel).filter(
            UserModel.email == email).first()
        if user and user.verification_code == verification_code:
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
