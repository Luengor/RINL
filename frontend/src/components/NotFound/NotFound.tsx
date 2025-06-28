import { Button, Center, Group, Stack, Text, Title } from "@mantine/core";
import classes from "./NotFound.module.css";
import { Link } from "react-router-dom";
import { useRef } from "react";
import useBackground from "../../hooks/useBackground";

export function NotFoundPage() {
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
      <Center className={classes.root} h="80vh">
        <Stack justify="center" align="center">
          <Title className={classes.label}>404</Title>
          <Title className={classes.title}>Aquí no hay nada.</Title>
          <Text
            c="dimmed"
            size="lg"
            ta="center"
            className={classes.description}
          >
            La página que estás buscando no existe. Puede que haya sido
            eliminada, o que nunca haya existido.
          </Text>
          <Group justify="center">
            <Button component={Link} to="/" variant="subtle" size="md">
              Volver a la página principal
            </Button>
          </Group>
        </Stack>
      </Center>
    </>
  );
}
