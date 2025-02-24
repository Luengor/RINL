import {
  Button,
  Container,
  Paper,
  PasswordInput,
  Text,
  TextInput,
  Title,
} from '@mantine/core';
import { useForm, isEmail, hasLength } from '@mantine/form'

import { ReactElement } from 'react';

import { login } from '../utils/session';
import { useNavigate } from 'react-router-dom';
  
export function LoginForm({ registrationLink }: { registrationLink: ReactElement }) {
  const navigate = useNavigate(); 
  const form = useForm({
    mode: 'uncontrolled',
    validate: {
      email: isEmail('Correo inválido'),
      password: hasLength({ min: 6 }, 'La contraseña debe tener al menos 6 caracteres'),
    }
  })

  function handleSubmit() {
    const { email, password } = form.getValues();
    login(email as string, password as string);
    navigate('/');
  }

  return (
    <Container size={420} my={40}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Title ta="center">
          Inicia sesión
        </Title>
        <Text c="dimmed" size="sm" ta="center" mt={5}>
          ¿No tienes cuenta?{' '}
          { registrationLink }
        </Text>

        <Paper withBorder shadow="md" p={30} mt={30} radius="md">
          <TextInput
            name='email'
            key={form.key('email')}
            label="Correo"
            placeholder="correo@corr.eo"
            required
            {...form.getInputProps('email')}
          />

          <PasswordInput
            name="password"
            key={form.key('password')}
            label="Contraseña"
            placeholder="Contraseña"
            required
            mt="md"
            {...form.getInputProps('password')}
          />
          <Button type="submit" fullWidth mt="xl">
            Iniciar sesión
          </Button>
        </Paper>
      </form>
    </Container>
  );
}
