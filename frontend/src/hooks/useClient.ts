import {
  ClientOptions,
  createClient,
  createConfig,
} from "@hey-api/client-fetch";
import { useRef } from "react";
import { loginForTokenLoginPost } from "../client";
import { useNavigate } from "react-router-dom";

export function useClient() {
  // API url
  const apiUrl = import.meta.env.VITE_API_URL || "http://localhost:8000";

  // Create a new client if it doesn't exist
  const clientRef = useRef(
    createClient(
      createConfig<ClientOptions>({
        baseUrl: apiUrl,
        throwOnError: true,
      })
    )
  );

  const navigate = useNavigate();

  // Logout the user if the token is invalid
  clientRef.current.interceptors.response.use(async (response) => {
    if (response.status === 401) {
      // Unauthorized, logout
      logout();

      // Redirect to login page
      navigate("/");
    }

    return response;
  });

  function getToken() {
    return localStorage.getItem("access_token");
  }
  if (getToken() !== null) {
    // Set the token in the client if it exists
    clientRef.current.setConfig({
      auth: localStorage.getItem("access_token"),
    });
  } else {
    // If no token, set auth to null
    clientRef.current.setConfig({
      auth: null,
    });
  }

  // Login function
  const login = async (email: string, password: string) => {
    const response = await loginForTokenLoginPost({
      client: clientRef.current,
      body: {
        grant_type: "password",
        username: email,
        password: password,
      },
    });

    localStorage.setItem("access_token", response.data.access_token);
    clientRef.current.setConfig({
      auth: response.data.access_token,
    });
  };

  // Logout function
  const logout = () => {
    localStorage.removeItem("access_token");
    clientRef.current.setConfig({
      auth: null,
    });
  };

  // Check if the user is logged in
  function loggedIn() {
    const token = getToken();
    if (token === null) {
      return false;
    }

    // Check if the expiration date of the token is in the past
    const payload = JSON.parse(atob(token.split(".")[1]));
    const expirationDate = new Date(payload.exp * 1000);
    if (expirationDate < new Date()) {
      // Token is expired, remove it
      logout();
      return false;
    }

    // Token is valid
    return true;
  }

  // Return everything
  return { client: clientRef.current, login, logout, loggedIn };
}
