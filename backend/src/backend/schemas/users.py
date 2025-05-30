from pydantic import BaseModel, ConfigDict
from typing import Optional

class UserBase(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    email: str
    verified: bool
    name: str
    year_of_birth: int

class ModifyUser(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    email: Optional[str] = None
    name: Optional[str] = None
    year_of_birth: Optional[int] = None

class RegisterUser(BaseModel):
    email: str
    password: str
    name: str
    year_of_birth: int

class UserFull(UserBase):
    password: str
    verification_code: str

