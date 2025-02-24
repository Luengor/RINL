from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

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
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"]
)

app.include_router(auth_router)
app.include_router(users_router)

