from .utils import client, login_token, session, engine
from .data import test_user
from datetime import datetime 
from schemas.users import UserBase
import pytest

@pytest.fixture
def create_activity(client, login_token):
    # Create an activity
    d = datetime.today().isoformat('T')
    response = client.post(
        "/activity/",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        },
        json={
            "date": d, 
            "minigame": "test",
            "duration": 100,
            "score": 100,
            "activity_points": 100,
            "extra_data": "{}"
        }
    )

    return response.json()


def test_create_activity(client, login_token):
    # Create an activity
    d = datetime.today().isoformat('T')
    response = client.post(
        "/activity/",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        },
        json={
            "date": d, 
            "minigame": "test",
            "duration": 100,
            "score": 100,
            "activity_points": 100,
            "extra_data": "{}"
        }
    )

    assert response.status_code == 200

    assert response.json() == {
        "date": d, 
        "uuid": 1,
        "minigame": "test",
        "duration": 100,
        "score": 100,
        "activity_points": 100,
        "extra_data": "{}",
        "user": UserBase(**test_user.model_dump()).model_dump()
    }
        
def test_get_activity(client, login_token, create_activity):
    # Get an activity
    response = client.get(
        f"/activity/",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    print(response.json())
    create_activity.pop("user")
    assert response.json() == [create_activity] 

def test_get_filter_minigate(client, login_token, create_activity):
    # Get an activity
    response = client.get(
        f"/activity/?minigame_filter=test",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    assert len(response.json()) == 1

    response = client.get(
        f"/activity/?minigame_filter=wrong",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    assert len(response.json()) == 0

def test_get_filter_date(client, login_token, create_activity):
    # Get an activity
    response = client.get(
        f"/activity/?from_date=2000-01-01",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    assert len(response.json()) == 1

    response = client.get(
        f"/activity/?to_date=2000-01-01",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    assert len(response.json()) == 0

def test_delete_activity(client, login_token, create_activity):
    # Delete an activity
    response = client.delete(
        f"/activity/{create_activity['uuid']}",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    create_activity.pop("user")
    create_activity.pop("uuid")
    assert response.json() == create_activity

    response = client.get(
        f"/activity/",
        headers={
            "Authorization": f"Bearer {login_token}",
            "Content-Type": "application/json"
        }
    )

    assert response.status_code == 200
    assert len(response.json()) == 0

