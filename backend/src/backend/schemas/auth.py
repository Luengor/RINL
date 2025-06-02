"""Authentication models for validating and serializing authentication data."""

from pydantic import BaseModel


class Token(BaseModel):
    """Pydantic model for authentication token data."""

    access_token: str
    """The access token for the user."""
    token_type: str
    """The type of the token, typically 'bearer'."""


class UserAuth(BaseModel):
    """Pydantic model for user authentication data."""

    uuid: int
    """The unique identifier for the user."""
    email: str
    """The email address of the user."""
    hashed_password: str
    """The hashed password of the user."""
