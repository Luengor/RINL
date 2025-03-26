import { ClientOptions, createClient, createConfig } from "@hey-api/client-fetch";
import { useRef } from "react";
import { loginForTokenLoginPost } from "../client";
import { notifications } from "@mantine/notifications";

export function useClient() {
  // Create a new client if it doesn't exist
  const clientRef = useRef(createClient(createConfig<ClientOptions>({
    baseUrl: 'http://localhost:8000',
    throwOnError: true,
  })));

  // Logout the user if the token is invalid
  clientRef.current.interceptors.response.use((response) => {
    if (response.status === 401) {
      // Unauthorized, remove the token
      localStorage.removeItem('access_token');
      clientRef.current.setConfig({
        auth: null
      });
    }

    return response;
  });

  // Set the token if it exists
  const token = localStorage.getItem('access_token');
  if (token) {
    clientRef.current.setConfig({
      auth: token
    });
  }

  // Login function
  const login = async (email: string, password: string) => {
    const response = await loginForTokenLoginPost({
      client: clientRef.current,
      body: {
        grant_type: 'password',
        username: email,
        password: password
      }
    });

    localStorage.setItem('access_token', response.data.access_token);
    clientRef.current.setConfig({
      auth: response.data.access_token
    });
  }

  // Logout function
  const logout = () => {
    localStorage.removeItem('access_token');
    clientRef.current.setConfig({
      auth: null
    });
  }

  // Logged in
  const loggedIn = !!clientRef.current.getConfig().auth;

  // Return everything
  return { client: clientRef.current, login, logout, loggedIn };
}