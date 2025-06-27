import { Grid, Loader, Stack, Text, Title } from "@mantine/core";

import { useUser } from "../../hooks/useUser";
import DataForm from "../../components/User/DataForm";
import CurrentShapeCard from "../../components/User/CurrentShapeCard";
import { useRef } from "react";
import useBackground from "../../hooks/useBackground";

export default function Data() {
  // User Data
  const { user, userStatus } = useUser();

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const isMobile = window.innerWidth <= window.innerHeight;
  useBackground(
    {
      canvasRef,
      text: "Tus datos",
      fontSize: isMobile ? 40 : 80,
      columnSep: isMobile ? 40 : 80,
      scrollSpeed: 0.6,
      randomSpeedMagnitude: 0.1,
    }
  );

  if (userStatus === "pending") {
    return <Loader type="dots" size="xl" />;
  } else if (userStatus === "error") {
    return <Text>Error al cargar los datos</Text>;
  }

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
      <Stack align="stretch">
        <Title order={1}>Mis datos</Title>
        <Text>Aquí puedes ver y modificar tus datos.</Text>
        <Grid>
          <Grid.Col span={{ base: 12, lg: 6 }}>
            <DataForm user={user} />
          </Grid.Col>
          <Grid.Col span={{ base: 12, lg: 6 }}>
            <CurrentShapeCard />
          </Grid.Col>
        </Grid>
      </Stack>
    </>
  );
}
