import {
  Button,
  Card,
  Center,
  Group,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { LoginForm } from "../components/LoginForm";
import { useNavigate } from "react-router-dom";
import { useClient } from "../hooks/useClient";
import { useEffect, useRef, useState } from "react";
import useBackground from "../hooks/useBackground";

// Pass as props to the UserCard component
interface UserCardProps {
  logout: () => void;
}

function UserCard({ logout }: UserCardProps) {
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/");
  }

  return (
    <Center w="100%" h="100%">
      <Card shadow="sm" padding="lg" radius="md" withBorder>
        <Stack>
          <Title order={2}>Hola de nuevo!</Title>
          <Button
            onClick={() => navigate("/my/data")}
            variant="light"
            fullWidth
          >
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
  if (loggedIn()) {
    login_content = <UserCard logout={logout} />;
  } else {
    login_content = <LoginForm />;
  }

  // spin
  const [angle, setAngle] = useState(69);

  useEffect(() => {
    const intervalId = setInterval(() => {
      setAngle((prevAngle) => (prevAngle + 1) % 360);
    }, 20);

    return () => clearInterval(intervalId);
  }, []);

  // weird phrases
  const phrases = [
    "¡Prepara la cámara!",
    "¡Prepara la toalla!",
    "¡No olvides beber agua!",
    "¡Más divertido que salir a correr! (subjetivo)",
    "¡No olvides estirar!",
    "¡No olvides calentar!",
    "Día de pierna :(",
  ];

  const [randomPhrase] = useState(Math.floor(Math.random() * phrases.length));

  // Canvas ref
  const canvasRef = useRef<HTMLCanvasElement>(null);
  useBackground({ canvasRef, repetitions: 2 });

  return (
    <>
      <canvas
        style={{
          position: "absolute",
          top: 0,
          left: 0,
          width: "100%",
          height: "100%",
          zIndex: -1,
        }}
        ref={canvasRef}
      />
      <Group h="100vh" grow preventGrowOverflow={false}>
        <Center h="100%" w="50%">
          <Stack align="left" justify="left" gap={0} pr={{ base: 0, lg: 200 }}>
            <Title
              order={1}
              size={120}
              fw={900}
              style={{ lineHeight: 1, marginBottom: 0 }}
            >
              <Text
                inherit
                variant="gradient"
                gradient={{ from: "orange", to: "yellow", deg: angle }}
              >
                RINL
              </Text>
            </Title>
            <Text
              size="xl"
              style={{
                fontFamily: "Helvetica, Arial, sans-serif",
                fontWeight: "900",
              }}
            >
              {phrases[randomPhrase]}
            </Text>
          </Stack>
        </Center>
        <Center>{login_content}</Center>
      </Group>
    </>
  );
}
