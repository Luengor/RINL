// const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:3001';
const API_URL = 'http://localhost:8000';

export async function login(email: string, password: string) {
  const request_options = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: new URLSearchParams({
      grant_type: 'password',
      username: email,
      password: password
    })
  }

  const response = await fetch(`${API_URL}/token`, request_options);
  const data = await response.json();

  if (!response.ok) {
    return null;
  }

  console.log(response);
  return data;
}
