import {
    Button,
    Container,
    Paper,
    PasswordInput,
    Text,
    TextInput,
    Title,
  } from '@mantine/core';

import { ReactElement } from 'react';
  
export function LoginForm({ registrationLink }: { registrationLink: ReactElement }) {
  return (
    <Container size={420} my={40}>
      <form>
        <Title ta="center">
          Inicia sesión
        </Title>
        <Text c="dimmed" size="sm" ta="center" mt={5}>
          ¿No tienes cuenta?{' '}
          { registrationLink }
          {/*
          <Anchor size="sm" component="button">
            Create account
          </Anchor>
          */}
        </Text>

        <Paper withBorder shadow="md" p={30} mt={30} radius="md">
          <TextInput name='email' label="Correo" placeholder="correo@corr.eo" required />
          <PasswordInput name="password" label="Contraseña" placeholder="Contraseña" required mt="md" />
          <Button type="submit" fullWidth mt="xl">
            Iniciar sesión
          </Button>
        </Paper>
      </form>
    </Container>
  );
}