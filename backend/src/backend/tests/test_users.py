# pylint: disable=missing-function-docstring,missing-module-docstring
import json
from fastapi.testclient import TestClient
from sqlalchemy.orm import Session
from backend.models.user import User
from backend.core.auth import NOT_VERIFIED
from backend.core.auth_utils import create_access_token
from .data import test_user, test_unverified_user


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


def test_create_user_already_exists(client: TestClient, verified_login_token: str):
    # Create a duplicate user
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
    assert response.status_code == 400
    assert response.json()["detail"] == "User already exists"


def test_get_me(client: TestClient, verified_login_token: str):
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


def test_login(client: TestClient, verified_login_token: str):
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


def test_verify_user(client: TestClient, unverified_login_token: str):
    verify_token = create_access_token(
        data={
            "sub": test_unverified_user.email,
            "type": "verify",
        }
    )

    # Verify user
    response = client.post(
        f"/user/me/email-token?token={verify_token}",
        headers={"Authorization": f"Bearer {unverified_login_token}"},
    )

    assert response.status_code == 200

    # Get the user and check if verified
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {unverified_login_token}"}
    )
    assert response.status_code == 200
    assert response.json() == {
        "email": test_unverified_user.email,
        "name": test_unverified_user.name,
        "year_of_birth": test_unverified_user.year_of_birth,
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


def test_try_update_email(client: TestClient, verified_login_token: str):
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
        "email": test_user.email,  # Email should not change
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": test_user.verified,
    }


def test_update_email(client: TestClient, verified_login_token: str):
    # Create update email token
    update_email_token = create_access_token(
        data={
            "sub": test_user.email,
            "type": "update_email",
            "new_email": "test2",
        }
    )

    # Verify user with update email token
    response = client.post(
        f"/user/me/email-token?token={update_email_token}",
        headers={"Authorization": f"Bearer {verified_login_token}"},
    )

    assert response.status_code == 200

    # Check if the user email was updated
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": "test2",
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": test_user.verified,
    }


def test_update_email_already_exists(client: TestClient, verified_login_token: str, unverified_login_token: str):
    # Create update email token
    update_email_token = create_access_token(
        data={
            "sub": test_user.email,
            "type": "update_email",
            "new_email": test_unverified_user.email,  # This email already exists
        }
    )

    # Try to verify user with update email token
    response = client.post(
        f"/user/me/email-token?token={update_email_token}",
        headers={"Authorization": f"Bearer {verified_login_token}"},
    )

    # Check response
    assert response.status_code == 400

    # Check data did not change
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )
    assert response.status_code == 200
    assert response.json().get("email") == test_user.email


def test_update_unverified_user(client: TestClient, unverified_login_token: str):
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
    assert not session.query(User).filter(
        User.email == test_user.email).first()


def test_get_after_delete_user(client: TestClient, verified_login_token: str):
    # Delete user
    response = client.delete(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )
    assert response.status_code == 200

    # Try to get user
    response = client.get(
        "/user/me", headers={"Authorization": f"Bearer {verified_login_token}"}
    )
    assert response.status_code == 404
    assert response.json()["detail"] == "User not found"
