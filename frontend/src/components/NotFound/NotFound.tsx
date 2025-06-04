import { Button, Center, Group, Stack, Text, Title } from '@mantine/core';
import classes from './NotFound.module.css';
import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <Center className={classes.root} h="80vh">
      <Stack justify="center" align="center">
        <Title className={classes.label}>404</Title>
        <Title className={classes.title}>Aquí no hay nada.</Title>
        <Text c="dimmed" size="lg" ta="center" className={classes.description}>
          La página que estás buscando no existe. Puede que haya sido eliminada, o que nunca haya existido.
        </Text>
        <Group justify="center">
          <Button component={Link} to="/" variant="subtle" size="md">
            Volver a la página principal
          </Button>
        </Group>
      </Stack>
    </Center>
  );
}