// const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:3001';
const API_URL = 'http://localhost:8000';

import { loginForTokenLoginPost } from "../client"; 

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
  const response = await loginForTokenLoginPost({
    body: {
      grant_type: 'password',
      username: email,
      password: password
    }
  })

  localStorage.setItem('access_token', response.data.access_token);

  console.log(response);
  return response.data;
}
