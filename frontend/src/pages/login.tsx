import { LoginForm } from "../components/LoginForm";
import { Center } from "@mantine/core";
import { useClient } from "../hooks/useClient";
import { useNavigate } from "react-router-dom";

function Login() {
  const { loggedIn } = useClient();
  const navigate = useNavigate();

  // Go to /my if we have a token
  if (loggedIn)
    navigate("/my/data");

  return (
    <Center>
      <LoginForm />
    </Center>
  );
}

export default Login;
