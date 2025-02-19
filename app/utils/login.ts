"use server";

import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

const API_URL = 'http://localhost:8000/';

export async function login(data: FormData) {
  // Username and password
  const username: string = data.get("username") as string;
  const password: string = data.get("password") as string;

  // Log in
  const request_options: RequestInit = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: new URLSearchParams({
      grant_type: 'password',
      username: username,
      password: password
    })
  };

  const response: Response = await fetch(API_URL + 'token', request_options);
  if (!response.ok) {
    redirect('/login?failed=true');
  }

  const token: string = (await response.json()).access_token ?? '';

  const base64Url = token.split('.')[1];
  const base64 = base64Url.replace('-', '+').replace('_', '/');
  const jwt = JSON.parse(atob(base64));
  const exp = new Date(jwt.exp * 1000);

  (await cookies()).set('access_token', token, {
    httpOnly: true,
    secure: true,
    expires: exp,
    sameSite: 'lax'
  });

  redirect('/media');
}

export async function logout() {
  (await cookies()).delete('access_token');
  redirect('/login');
}

export async function getTokenSession() {
  // Check if we have a session token
  const token = (await cookies()).get('access_token');
  if (!token) {
    return null;
  }

  // Check the token with the api
  const request_options: RequestInit = {
    method: 'POST',
    headers: {
      'accept': 'application/json',
      'Authorization': 'Bearer ' + token
    }
  }

  const response: Response = await fetch(API_URL + 'token/verify', request_options);
  return response.ok ? token : null;
}
