from .utils import client 

def test_get_nothing(client):
    response = client.get("/shape/")
    assert response.status_code == 200
    assert response.json() == []
    