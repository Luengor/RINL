import {
  ClientOptions,
  createClient,
  createConfig,
} from "@hey-api/client-fetch";
import { useRef, useState } from "react";
import { loginForTokenLoginPost } from "../client";

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

  // Logout the user if the token is invalid
  clientRef.current.interceptors.response.use(async (response) => {
    if (response.status === 401 && clientRef.current.getConfig().auth) {
      // Unauthorized, remove the token
      localStorage.removeItem("access_token");
      clientRef.current.setConfig({
        auth: null,
      });
    }

    return response;
  });

  function hasToken() {
    return !!localStorage.getItem("access_token");
  }
  if (hasToken()) {
    // Set the token in the client if it exists
    clientRef.current.setConfig({
      auth: localStorage.getItem("access_token"),
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
    return hasToken();
  }

  // Return everything
  return { client: clientRef.current, login, logout, loggedIn };
}
