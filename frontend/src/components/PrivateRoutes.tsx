import { Navigate, Outlet } from "react-router-dom";
import { useClient } from "../hooks/useClient";

interface PrivateRoutesProps {
  requireLogin: boolean;
  route: string;
}

export function PrivateRoutes({ requireLogin, route }: PrivateRoutesProps) {
  const { loggedIn } = useClient();
  const logged_in = requireLogin ? loggedIn : !loggedIn;

  return logged_in ? <Outlet /> : <Navigate to={route} />;
}
