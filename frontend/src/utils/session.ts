import { loginForTokenLoginPost, refreshTokenLoginRefreshPost, createUserUserPost } from "../client"; 
import { client } from "../client/client.gen";

export async function login(email: string, password: string) {
  const response = await loginForTokenLoginPost({
    body: {
      grant_type: 'password',
      username: email,
      password: password
    }
  })

  if (response.error)
    throw response.error;

  localStorage.setItem('access_token', response.data.access_token);
  client.setConfig({
    headers: {
      "Authorization": `Bearer ${response.data.access_token}`
    }
  })
}

export function logout() {
  // Remove the access token
  localStorage.removeItem('access_token');

  client.setConfig( { headers: { "Authorization": "" } } )
}

export async function register(email: string, password: string, name: string, birthYear: number) {
  // Register a new user
  const response = await createUserUserPost({
    body: {
      email: email,
      password: password,
      name: name,
      year_of_birth: birthYear
    }
  });

  if (response.error)
    throw response.error;

  return await login(email, password);
}

export function has_token() {
  // Check if we have an access token
  return localStorage.getItem('access_token') !== null;
}

export async function refresh_token() {
  // Refresh the access token
  const response = await refreshTokenLoginRefreshPost();
  localStorage.setItem('access_token', response.data.access_token);
  return response.data;
}

