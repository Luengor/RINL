from os import environ
from sqlalchemy import create_engine
from sqlalchemy.orm import DeclarativeBase 

ENGINE_PATH = f"postgresql://{environ.get('POSTGRES_USER')}:{environ.get('POSTGRES_PASSWORD')}@{environ.get('POSTGRES_HOST')}:5432"
engine = create_engine(ENGINE_PATH)

class Base(DeclarativeBase):
    pass 
