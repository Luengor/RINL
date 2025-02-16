from pydantic import BaseModel

class User(BaseModel):
    email: str
    verified: bool
    verification_code: str
    name: str
    year_of_birth: int

