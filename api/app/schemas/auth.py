from pydantic import BaseModel

class Token(BaseModel):
    access_token: str
    token_type: str

class UserAuth(BaseModel):
    email: str
    hashed_password: str
