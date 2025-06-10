"""Main module defining the FastAPI application that includes all the routers"""
from os import environ
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from backend.routers.auth import router as auth_router
from backend.routers.user import router as users_router
from backend.routers.activity import router as activity_router
from backend.routers.shape import router as shape_router

app = FastAPI(
    title="RINL",
    root_path="/api",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[environ.get("CORS_ORIGIN", "*")],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth_router)
app.include_router(users_router)
app.include_router(activity_router)
app.include_router(shape_router)


def main():
    """Main function that runs the FastAPI application"""
    import uvicorn
    uvicorn.run("app", host="localhost", port=8000,
                log_level="info", reload=True)


if __name__ == "__main__":
    main()
