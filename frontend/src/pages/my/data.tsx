import {
  Button,
  Center,
  Chip,
  Collapse,
  Loader,
  Modal,
  NumberInput,
  PinInput,
  Stack,
  Text,
  TextInput,
  Title
} from '@mantine/core';

import { useEffect, useState } from 'react';
import { getMeUserMeGet } from '../../client';
import { useQuery } from '@tanstack/react-query';
import { IconMail, IconUser, IconCalendar, IconCheck, IconX } from '@tabler/icons-react';
import { useForm, Form, hasLength } from '@mantine/form';
import { client } from '../../client/client.gen';
import { notifications } from '@mantine/notifications';

export default function Data() {
  // Data
  const { data, refetch, status } = useQuery({
    queryKey: ['user-data'],
    queryFn: async () => {
      const req = await getMeUserMeGet();
      return req.data;
    },
    staleTime: 1000 * 60 * 5,
  })

  // Verify form
  const [verifing, setVerifing] = useState(false);
  const [fetching_verifying, setFetchingVerifying] = useState(false);
  const verifyForm = useForm({
    name: 'verify-form',
    mode: 'uncontrolled',
    validate: {
      pin: hasLength({ min: 6, max: 6 }, 'El código debe tener 6 dígitos')
    },
  });

  const handleVerify = async () => {
    const pin = (verifyForm.getValues().pin as string).toUpperCase();
    setFetchingVerifying(true);
    const req = await client.post({
      url: '/user/verify/' + pin,
      security: [{ scheme: 'bearer', type: 'http' }]
    })
    setFetchingVerifying(false);

    if (req.error) {
      notifications.show({
        title: 'Error',
        message: 'No se ha podido verificar tu correo electrónico',
        color: 'red',
        icon: <IconX />
      });
      verifyForm.setErrors({ pin: 'Código incorrecto' });
      return;
    }

    notifications.show({
      title: 'Correo verificado',
      message: 'Tu correo electrónico ha sido verificado correctamente',
      color: 'green',
      icon: <IconCheck/>
    });
    setVerifing(false);
    refetch();
  }

  // Main form
  const dataForm = useForm({
    name: 'data-form',
    mode: 'uncontrolled',
    validate: {
      name: hasLength({ min: 1, max: 255 }, 'Nombre no puede estar vacío'),
      email: hasLength({ min: 1, max: 255 }, 'Correo no puede estar vacío'),
      year_of_birth: (value) => (value < 1900 || value > Date.now() ? 'Año de nacimiento inválido' : null) 
    }
  });
  const [updating, setUpdating] = useState(false);
  const handleModify = async () => {
    const {name, email, year_of_birth} = dataForm.getValues();
    setUpdating(true);
    alert("There is no endpoint to update the user data yet :)");
  }

  useEffect(() => {
    if (data) {
      dataForm.setValues({
        name: data.name,
        email: data.email,
        year_of_birth: data.year_of_birth,
      });
      dataForm.setInitialValues({
        name: data.name,
        email: data.email,
        year_of_birth: data.year_of_birth,
      });
    }
  }, [status])

  let dataTsx;
  if (status === 'pending') {
    dataTsx = (
      <Loader type="dots" size="xl"/>
    );
  }
  else if (status === 'error') {
    dataTsx = (
      <Text>Error al cargar los datos</Text>
    );
  } else if (status === 'success') {
    dataTsx = (
      <>
      <Form form={dataForm} onSubmit={handleModify}>
        <Stack align='stretch' gap="sm">
          <TextInput
            label="Nombre"
            key={dataForm.key('name')}
            {...dataForm.getInputProps('name')}
            leftSection={<IconUser />}
            placeholder='Nombre'
            readOnly={!data.verified}
          />
          <NumberInput
            label="Año de nacimiento"
            key={dataForm.key('year_of_birth')}
            {...dataForm.getInputProps('year_of_birth')}
            leftSection={<IconCalendar />}
            placeholder='2000'
            readOnly={!data.verified}
          />
          <Stack gap="0">
            <TextInput
              label="Correo electrónico"
              key={dataForm.key('email')}
              {...dataForm.getInputProps('email')}
              leftSection={<IconMail />}
              rightSection={
                <Chip readOnly checked={data.verified}>
                  { data.verified ? 'Verificado' : 'Sin verificar' }
                </Chip>
              }
              rightSectionWidth={data.verified ? 110 : 125}
              placeholder='ejemplo@ejemp.lo'
              readOnly={!data.verified}
            />
            <Collapse in={!data.verified}>
                <Text c="dimmed" size="sm" span>
                  Verifica tu correo electrónico
                </Text>
                <Text style={{ cursor: 'pointer' }} size="sm" span c="blue" onClick={() => setVerifing(true)}>{' aquí.'}</Text>
            </Collapse>
          </Stack>
          <Collapse in={dataForm.isDirty()}>
              <Center>
                <Button loading={updating} type='submit'>Modificar datos</Button>
              </Center>
          </Collapse>
        </Stack>
      </Form>
      <Modal opened={verifing} title="Verificar correo electrónico" onClose={() => setVerifing(false)} centered>
        <Form form={verifyForm} onSubmit={handleVerify}>
          <Stack align='center'>
              <Text>Introduce el código de verificación que te hemos enviado a {data.email}</Text>
              <PinInput
                name='pin'
                key={verifyForm.key('pin')}
                {...verifyForm.getInputProps('pin')}
                length={6}
                oneTimeCode
                />
              <Button loading={fetching_verifying} mt="sm" type='submit' variant="filled">Verificar</Button>
          </Stack>
        </Form>
      </Modal>
      </>
    );
  }

  return (
    <>
    <Stack maw="50em" align='stretch'>
      <Title order={1}>Mis datos</Title>
      <Text >
        Aquí puedes ver y modificar tus datos.
      </Text>
      {dataTsx}
    </Stack>
    </>
  )
}
