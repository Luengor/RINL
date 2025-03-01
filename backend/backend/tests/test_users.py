from .utils import client, session, login_token
from .data import test_user
from models.user import User

def test_create_user(client):
    # Create a user
    response = client.post(
        "/user/",
        json = {
            "email": test_user.email,
            "password": test_user.password,
            "name": test_user.name,
            "year_of_birth": test_user.year_of_birth
        }
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": False
    }

    # Check user in database
    assert session.query(User).filter(User.email == test_user.email).first()

def test_get_me(client, login_token):
    # Get user
    response = client.get(
        "/user/me",
        headers = {
            "Authorization": f"Bearer {login_token}"
        }
    )

    # Check response
    assert response.status_code == 200
    assert response.json() == {
        "email": test_user.email,
        "name": test_user.name,
        "year_of_birth": test_user.year_of_birth,
        "verified": True 
    }

