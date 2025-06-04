import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App.tsx";

import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/notifications/styles.css";

import { createTheme, MantineProvider } from "@mantine/core";
import { Notifications } from "@mantine/notifications";
import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { ErrorNotification } from "./utils/notifications";

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error, query) => {
      console.log("Error in query: ", error.message);

      if (query.meta.errorMessage)
        ErrorNotification(query.meta.errorMessage as string);
    },
  }),

  mutationCache: new MutationCache({
    onError: (error, variables, context, mutation) => {
      console.log("Error in mutation: ", error, variables, context, mutation);

      if (mutation.meta.errorMessage)
        ErrorNotification(mutation.meta.errorMessage as string);
    },
  }),
});

const theme = createTheme({
  primaryColor: "orange",
})

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
  <React.StrictMode>
    <MantineProvider theme={theme}>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <Notifications />
          <App />
        </QueryClientProvider>
      </BrowserRouter>
    </MantineProvider>
  </React.StrictMode>
);
