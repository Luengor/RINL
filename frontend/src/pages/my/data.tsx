import {
  Button,
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

import { useState } from 'react';
import { getMeUserMeGet } from '../../client';
import { useQuery } from '@tanstack/react-query';
import { IconMail, IconUser, IconCalendar, IconCheck, IconX } from '@tabler/icons-react';
import { useForm, Form, hasLength } from '@mantine/form';
import { client } from '../../client/client.gen';
import { notifications } from '@mantine/notifications';

export default function Data() {
  // Data
  const { isPending, data, refetch } = useQuery({
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
  }) 

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

  let dataTsx;
  if (isPending) {
    dataTsx = (
      <Loader type="dots" size="xl"/>
    );
  } else {
    dataTsx = (
      <>
        <TextInput
          label="Nombre"
          leftSection={<IconUser />}
          placeholder='Nombre'
          value={data.name}
          readOnly
        />
        <NumberInput
          label="Año de nacimiento"
          leftSection={<IconCalendar />}
          placeholder='2000'
          value={data.year_of_birth}
          readOnly
        />
        <Stack gap="0">
          <TextInput
            label="Correo electrónico"
            leftSection={<IconMail />}
            rightSection={
              <Chip readOnly checked={data.verified}>
                { data.verified ? 'Verificado' : 'Sin verificar' }
              </Chip>
            }
            rightSectionWidth={data.verified ? 110 : 125}
            placeholder='ejemplo@ejemp.lo'
            value={data.email}
            readOnly
          />
          <Collapse in={!data.verified}>
              <Text c="dimmed" size="sm" span>
                Verifica tu correo electrónico
              </Text>
              <Text style={{ cursor: 'pointer' }} size="sm" span c="blue" onClick={() => setVerifing(true)}>{' aquí.'}</Text>
          </Collapse>
        </Stack>
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
    <Stack py="xl" align='stretch'>
      <Title order={1}>Mis datos</Title>
      <Text >
        Aquí puedes ver y modificar tus datos.
      </Text>
      {dataTsx}
    </Stack>
    </>
  )
}
