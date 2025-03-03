import { loginForTokenLoginPost } from "../client"; 

export async function login(email: string, password: string) {
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

export function is_logged_in() {
  // Check if we have an access token
  return localStorage.getItem('access_token') !== null;
}
