from .utils import client, login_token
from .data import test_user
from datetime import datetime 
from schemas.users import UserBase

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
            "activity_points": 100,
            "extra_data": "{}"
        }
    )

    assert response.status_code == 200
    rjson = response.json()

    assert "uuid" in rjson.keys()
    rjson.pop("uuid")

    print(d)
    assert rjson == {
        "date": d, 
        "minigame": "test",
        "duration": 100,
        "activity_points": 100,
        "extra_data": "{}",
        "user": UserBase(**test_user.model_dump()).model_dump()
    }
        

