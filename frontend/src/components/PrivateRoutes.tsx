import { Navigate, Outlet } from "react-router-dom";
import { has_token } from "../utils/session";

interface PrivateRoutesProps {
  login: boolean;
  route: string;
}

export function PrivateRoutes({ login, route }: PrivateRoutesProps) {
  const logged_in = login ? has_token() : !has_token();

  return (
    logged_in ? <Outlet /> : <Navigate to={route} />
  );
}
