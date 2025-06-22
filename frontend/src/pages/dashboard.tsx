import { Button, Card, Center, Divider, Group, Stack, Title } from "@mantine/core";
import { LoginForm } from "../components/LoginForm";
import { useNavigate } from "react-router-dom";
import { useClient } from "../hooks/useClient";

// Pass as props to the UserCard component
interface UserCardProps {
  logout: () => void;
}

function UserCard({ logout }: UserCardProps) {
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/dashboard");
  }

  return (
    <Center w="100%" h="100%">
      <Card shadow="sm" padding="lg" radius="md" withBorder>
        <Stack>
          <Title order={2}>Hola de nuevo!</Title>
          <Button onClick={() => navigate("/my/data")} variant="light" fullWidth>
            Ir a mi perfil
          </Button>
          <Button onClick={handleLogout} variant="light" color="red" fullWidth>
            Cerrar sesión
          </Button>
        </Stack>
      </Card>
    </Center>
  );
}

export default function Dashboard() {
  const { logout, loggedIn } = useClient();

  // Login form or
  let login_content;
  if (loggedIn) {
    login_content = <UserCard logout={logout} />;
  }
  else {
    login_content = (
      <Center w="100%" h="100%">
        <LoginForm />
      </Center>
    );
  }

  return (
    <>
      <Group h="100vh" grow preventGrowOverflow={false}>
        <Center h="100%" w="50%">
          <Stack align="left" justify="center">
            <Title size='170'>
              RINL
            </Title>
            No sé que poner aquí la vd <br />
            Igual una fotito de fondo o algo <br />
            Un videito de alguien jugando
          </Stack>
        </Center>
        <Center>
          <Divider orientation="vertical" h="90vh" />
          {login_content}
        </Center>
      </Group>
    </>
  );
}
