import { Center, Divider, Group, Stack, Title } from "@mantine/core";
import { LoginForm } from "../components/LoginForm";

export default function Dashboard() {
  return (
    <>
      <Group h="100vh" grow preventGrowOverflow={false}>
        <Center h="100%" w="50%">
          <Stack align="center" justify="center">
            <Title size='170' order={1}>
              RINL
            </Title>
            No sé que poner aquí la vd <br />
            Igual una fotito de fondo o algo <br />
            Un videito de alguien jugando
          </Stack>
        </Center>
        <Center>
          <Divider orientation="vertical" h="90vh" />
          <LoginForm />
        </Center>
      </Group>
    </>
  );
}
