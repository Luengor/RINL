from pydantic import BaseModel

class Token(BaseModel):
    access_token: str
    token_type: str

class UserAuth(BaseModel):
    uuid: int
    email: str
    hashed_password: str
