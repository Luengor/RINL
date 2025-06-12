"""User models for handling validation and serialization of user data."""

from typing import Optional
from pydantic import BaseModel, ConfigDict


class UserBase(BaseModel):
    """Base pydantic model for user data."""
    model_config = ConfigDict(from_attributes=True)

    email: str
    """The email address of the user."""
    verified: bool
    """Indicates whether the user's email is verified."""
    name: str
    """The name of the user."""
    year_of_birth: int
    """The year of birth of the user."""


class ModifyUser(BaseModel):
    """Pydantic model for modifying user data."""
    model_config = ConfigDict(from_attributes=True)

    email: Optional[str] = None
    """The email address of the user."""
    name: Optional[str] = None
    """The name of the user."""
    year_of_birth: Optional[int] = None
    """The year of birth of the user."""


class RegisterUser(BaseModel):
    """Pydantic model for registering a new user."""

    email: str
    """The email address of the user."""
    password: str
    """The plain password for the user account."""
    name: str
    """The name of the user."""
    year_of_birth: int
    """The year of birth of the user."""


class UserFull(UserBase):
    """Pydantic model for full user data including additional fields."""

    password: str
    """The hashed password of the user."""
