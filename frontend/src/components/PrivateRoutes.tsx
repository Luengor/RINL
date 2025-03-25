import { Navigate, Outlet } from "react-router-dom";
import { useClient } from "../hooks/useClient";

interface PrivateRoutesProps {
  login: boolean;
  route: string;
}

export function PrivateRoutes({ login, route }: PrivateRoutesProps) {
  const { loggedIn } = useClient();
  const logged_in = login ? loggedIn : !loggedIn;

  return (
    logged_in ? <Outlet /> : <Navigate to={route} />
  );
}
