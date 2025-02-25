from pydantic import BaseModel, ConfigDict

class UserBase(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    email: str
    verified: bool
    name: str
    year_of_birth: int

class RegisterUser(BaseModel):
    email: str
    password: str
    name: str
    year_of_birth: int

class UserFull(UserBase):
    password: str
    verification_code: str

