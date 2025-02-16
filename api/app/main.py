from contextlib import asynccontextmanager

from fastapi import FastAPI

from routers.auth import router as auth_router
from routers.users import router as users_router
from core.db import Base, engine


@asynccontextmanager
async def lifespan(app: FastAPI):
    ## Startup
    # Create database tables
    Base.metadata.create_all(engine)

    yield

    ## Shutdown
    pass


app = FastAPI(lifespan=lifespan)

app.include_router(auth_router)
app.include_router(users_router)

