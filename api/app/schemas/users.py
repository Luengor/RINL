from pydantic import BaseModel

class UserBase(BaseModel):
    email: str
    verified: bool
    name: str
    year_of_birth: int

class RegisterUser(BaseModel):
    email: str
    password: str
    name: str
    year_of_birth: int

class FullUser(UserBase):
    password: str
    verification_code: str

