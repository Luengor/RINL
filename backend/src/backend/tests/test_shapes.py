# pylint: missing-function-docstring, missing-module-docstring
from typing import Any
from datetime import datetime
from fastapi.testclient import TestClient
import pytest
from backend.core.auth import NOT_VERIFIED
from backend.schemas.shape import ShapeBase
from .data import test_user

test_shape = ShapeBase(
    date=datetime.now(),
    weight=100,
    height=200,
)


@pytest.fixture
def create_shape(client: TestClient, verified_login_token: str) -> dict[str, Any]:
    # Create a shape
    response = client.post(
        "/shape/",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
        json=ShapeBase.model_dump(test_shape, mode="json"),
    )

    return response.json()


def test_create_shape(client: TestClient, verified_login_token: str):
    # Create a shape
    response = client.post(
        "/shape/",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
        json=ShapeBase.model_dump(test_shape, mode="json"),
    )
    assert response.status_code == 200
    response_json = response.json()
    response_json.pop("uuid")
    assert response_json == {
        "date": test_shape.date.isoformat(),
        "weight": test_shape.weight,
        "height": test_shape.height,
        "user": {
            "email": test_user.email,
            "name": test_user.name,
            "year_of_birth": test_user.year_of_birth,
            "verified": test_user.verified,
        },
    }


def test_get_shape(
    client: TestClient, verified_login_token: str, create_shape: dict[str, Any]
):
    # Get shape
    response = client.get(
        "/shape",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
    )

    assert response.status_code == 200
    response_json = response.json()
    assert len(response_json) == 1
    response_json = response_json[0]
    response_json.pop("uuid")
    assert response_json == {
        "date": test_shape.date.isoformat(),
        "weight": test_shape.weight,
        "height": test_shape.height,
        "user": {
            "email": test_user.email,
            "name": test_user.name,
            "year_of_birth": test_user.year_of_birth,
            "verified": test_user.verified,
        },
    }


def test_get_current_shape(
    client: TestClient, verified_login_token: str, create_shape: dict[str, Any]
):
    # Get shape
    response = client.get(
        "/shape/current",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
    )
    assert response.status_code == 200
    response_json = response.json()
    response_json.pop("uuid")
    assert response_json == {
        "date": test_shape.date.isoformat(),
        "weight": test_shape.weight,
        "height": test_shape.height,
        "user": {
            "email": test_user.email,
            "name": test_user.name,
            "year_of_birth": test_user.year_of_birth,
            "verified": test_user.verified,
        },
    }


def test_get_current_shape_no_shape(client: TestClient, verified_login_token: str):
    # Get shape
    response = client.get(
        "/shape/current",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
    )
    assert response.status_code == 404
    assert response.json()["detail"] == "Shape not found"


def test_create_shape_unverified_user(client: TestClient, unverified_login_token: str):
    # Create a shape
    response = client.post(
        "/shape/",
        headers={
            "Authorization": f"Bearer {unverified_login_token}",
        },
        json=ShapeBase.model_dump(test_shape, mode="json"),
    )
    assert response.status_code == 400
    assert response.json()["detail"] == NOT_VERIFIED.detail


def test_get_shape_unverified_user(
    client: TestClient, unverified_login_token: str, create_shape: dict[str, Any]
):
    # Get shape
    response = client.get(
        "/shape",
        headers={
            "Authorization": f"Bearer {unverified_login_token}",
        },
    )

    assert response.status_code == 400
    assert response.json()["detail"] == NOT_VERIFIED.detail


def test_get_current_shape_unverified_user(
    client: TestClient, unverified_login_token: str, create_shape: dict[str, Any]
):
    # Get shape
    response = client.get(
        "/shape/current",
        headers={
            "Authorization": f"Bearer {unverified_login_token}",
        },
    )
    assert response.status_code == 400
    assert response.json()["detail"] == NOT_VERIFIED.detail


def test_delete_shape(
    client: TestClient, verified_login_token: str, create_shape: dict[str, Any]
):
    # Delete shape
    response = client.delete(
        f"/shape/{create_shape['uuid']}",
        headers={
            "Authorization": f"Bearer {verified_login_token}",
        },
    )

    assert response.status_code == 200
    create_shape.pop("user")
    create_shape.pop("uuid")
    assert response.json() == create_shape
