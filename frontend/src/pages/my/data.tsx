import { Card, Center, Loader, Stack, Text, Title } from "@mantine/core";

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

  const pageWidth = window.innerWidth;
  useBackground({
    canvasRef,
    text: "Tus datos",
    fontSize: Math.max((pageWidth / 1920.0) * 70, 40),
    columnSep: Math.max((pageWidth / 1920.0) * 70, 40),
    scrollSpeed: 0.6,
    randomSpeedMagnitude: 0.1,
  });

  if (userStatus === "pending") {
    return <Loader type="dots" size="xl" />;
  } else if (userStatus === "error") {
    return <Text>Error al cargar los datos</Text>;
  }

  const page_contents = (
    <Stack align="stretch">
      <Title order={1}>Mis datos</Title>
      <Text>Aquí puedes ver y modificar tus datos.</Text>
      <DataForm user={user} />
      <CurrentShapeCard />
    </Stack>
  );

  let card: JSX.Element;
  if (isMobile) {
    card = page_contents;
  } else {
    card = (
      <Center h="100vh" w="100%">
        <Card
          shadow="sm"
          p="xl"
          radius="md"
          withBorder
          w={{ base: "100%", sm: "80%", xl: 1000 }}
          style={{ position: "relative", top: -30, zIndex: 1 }}
        >
          {page_contents}
        </Card>
      </Center>
    );
  }

  // Disable scroll on page if not on mobile
  if (!isMobile) {
    document.body.style.overflow = "hidden";
  } else {
    document.body.style.overflow = "auto";
  }

  return (
    <>
      {card}
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
    </>
  );
}
