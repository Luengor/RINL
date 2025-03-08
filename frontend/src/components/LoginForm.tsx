import {
  Button,
  Collapse,
  Container,
  Group,
  NumberInput,
  Paper,
  PasswordInput,
  Stack,
  Text,
  TextInput,
  Title,
} from '@mantine/core';
import { useForm, isEmail, hasLength } from '@mantine/form'
import { useDisclosure } from '@mantine/hooks';

import { Form } from '@mantine/form';
import { login, register as register_func } from '../utils/session';
import { useNavigate } from 'react-router-dom';
import type { HttpValidationError } from '../client';
import { has_token } from '../utils/session';
  
export function LoginForm() {
  const navigate = useNavigate(); 

  const [register, { toggle }] = useDisclosure(false) // State for the registration form;

  // Go to /my if we have a token
  if (has_token())
    navigate('/my/data');

  // Form validation
  const form = useForm({
    mode: 'uncontrolled',
    validate: {
      email: isEmail('Correo inválido'),
      password: hasLength({ min: 6 }, 'La contraseña debe tener al menos 6 caracteres')
    },
    initialValues : {
      email: '',
      password: '',
      name: '',
      birthYear: 2000 
    },
  })

  // Form submission
  async function handleSubmit() {
    const { email, password, name, birthYear } = form.getValues();

    try {
      if (register)
        await register_func(email, password, name, birthYear)
      else
        await login(email, password);

      navigate('/');
    } catch (err) {
      if ((err as HttpValidationError).detail) {
        const error = ((err as HttpValidationError).detail ?? 'Error desconocido') as string;
        form.setErrors({ email: error });
      }
    }
  }

  // Actual form
  return (
    <Container miw="350" w='25%' my={40}>
      <Form form={form} onSubmit={handleSubmit}>
        <Title ta="center">
          {register ? 'Regístrate' : 'Inicia sesión'}
        </Title>
        <Group justify='center' gap='xs' align='center'>
          <Text c="dimmed" size="sm" ta="center" mt={5}>
            {register ? '¿Ya tienes una cuenta?' : '¿No tienes una cuenta?'}
          </Text>
          <Text c="blue" size="sm" ta="center" mt={5} onClick={toggle} style={{ cursor: 'pointer' }}>
            {register ? 'Inicia sesión' : 'Regístrate'}
          </Text>
        </Group>

        <Paper withBorder shadow="md" p={30} mt={30} radius="md">
          <Stack gap='md'>
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
              {...form.getInputProps('password')}
            />

            <Collapse in={register} onTransitionEnd={form.clearErrors}>
              <Stack gap='md'>
                <TextInput
                  name='name'
                  key={form.key('name')}
                  label="Nombre"
                  placeholder="Nombre"
                  required
                  disabled={!register}
                  {...form.getInputProps('name')}
                />

                <NumberInput
                  name='birthYear'
                  key={form.key('birthYear')}
                  label="Año de nacimiento"
                  placeholder="2000"
                  min={1900}
                  max={new Date().getFullYear()}
                  required
                  disabled={!register}
                  {...form.getInputProps('birthYear')}
                />
              </Stack>
            </Collapse>
          </Stack>

          <Button type="submit" fullWidth mt="xl">
            {register ? 'Registrarse' : 'Iniciar sesión'}
          </Button>
        </Paper>
      </Form>
    </Container>
  );
}
