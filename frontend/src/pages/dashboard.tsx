import {
  Button,
  Card,
  Center,
  SimpleGrid,
  Space,
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
    "¡No te olvides de estirar!",
    "¡No te olvides de calentar!",
    "Día de pierna :(",
    "Si bebes, no conduzcas",
  ];

  const [randomPhrase] = useState(Math.floor(Math.random() * phrases.length));

  const isMobile = window.innerWidth < window.innerHeight;

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
      <Center h="100vh" w="100%">
        <SimpleGrid cols={isMobile ? 1 : 3} verticalSpacing="sm" spacing="md">
          <Stack
            align={isMobile ? "center" : "flex-start"}
            justify="center"
            gap={0}
          >
            <Title
              order={1}
              size={120}
              fw={900}
              ta={isMobile ? "center" : "left"}
              style={{
                lineHeight: 1,
                marginBottom: 0,
                marginLeft: 0,
                paddingLeft: 0,
              }}
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
              ta={isMobile ? "center" : "left"}
              style={{
                fontFamily: "Helvetica, Arial, sans-serif",
                fontWeight: "900",
              }}
            >
              {phrases[randomPhrase]}
            </Text>
          </Stack>
          {!isMobile && <Space />}
          {login_content}
        </SimpleGrid>
      </Center>
    </>
  );
}
