# pylint: disable=unused-import, unused-argument, redefined-outer-name, missing-function-docstring, missing-module-docstring
# pyright: reportUnusedImport=false
from fastapi.testclient import TestClient
from sqlalchemy.orm import Session
from backend.models.user import User
from backend.core.auth import NOT_VERIFIED
from .data import test_user, test_unverified_user
from .utils import client, session, verified_login_token, unverified_login_token, engine


def test_create_user(client: TestClient, session: Session):
    # Create a user
    response = client.post(
        "/user/",
        json={
            "email": test_user.email,
            "password": test_user.password,
            "name": test_user.name,
            "year_of_birth": test_user.year_of_birth,
        },
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": False,
    }

    # Check user in database
    assert session.query(User).filter(User.email == test_user.email).first()


def test_get_me(client: TestClient, verified_login_token: str, session: Session):
    # Get user
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": True,
    }


def test_login(client: TestClient, verified_login_token: str, session: Session):
    # Login user
    response = client.post(
        "/login",
        data={
            "username": test_user.email,
            "password": test_user.password,
        },
        headers={"Content-Type": "application/x-www-form-urlencoded"},
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "access_token": response.json()["access_token"],
        "token_type": "bearer",
    }


def test_verify_user(client: TestClient, unverified_login_token: str, session: Session):
    code = test_unverified_user.verification_code

    # Verify user
    response = client.post(
        f"/user/verify/{code}",
        headers={"Authorization": f"Bearer {unverified_login_token}"},
    )

    assert response.status_code == 200

    # Get the user and check if verified
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {unverified_login_token}"}
    )
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": True,
    }


def test_update_name(client: TestClient, verified_login_token: str):
    # Update user
    response = client.put(
        "/user/me",
        headers={"Authorization": f"Bearer {verified_login_token}"},
        json={
            "name": "test2",
            "year_of_birth": 2001,
        },
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": "test2",
        "year_of_birth": 2001,
        "verified": test_user.verified,
    }


def test_update_email(client: TestClient, verified_login_token: str):
    # Update user
    response = client.put(
        "/user/me",
        headers={"Authorization": f"Bearer {verified_login_token}"},
        json={
            "email": "test2",
        },
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": "test2",
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": False,  # New email, not verified
    }


def test_update_without_verification(client: TestClient, unverified_login_token: str):
    # Update user
    response = client.put(
        "/user/me",
        headers={"Authorization": f"Bearer {unverified_login_token}"},
        json={
            "email": "test2",
            "name": "test2",
            "year_of_birth": 2001,
        },
    )

    # Check response
    assert response.status_code == 400
    assert response.json()["detail"] == NOT_VERIFIED.detail


def test_delete_user(client: TestClient, verified_login_token: str, session: Session):
    # Delete user
    response = client.delete(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )
    assert response.status_code == 200

    # Check user in database
    assert not session.query(User).filter(User.email == test_user.email).first()
