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
import { IconMail, IconUser, IconCalendar } from '@tabler/icons-react';

export default function Data() {
  // Data
  const { isPending, data } = useQuery({
    queryKey: ['user-data'],
    queryFn: async () => {
      const req = await getMeUserMeGet();
      return req.data;
    },
    staleTime: 1000 * 60 * 5,
  })
  const [verifing, setVerifing] = useState(false);
  let current_pin = "";

  const handleVerify = () => {
    // TODO: Verify email
    console.log(current_pin);
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
          <Stack align='center'>
            <Text>Introduce el código de verificación que te hemos enviado a {data.email}</Text>
            <PinInput length={6} onChange={(value) => current_pin = value} oneTimeCode/>
            <Button mt="sm" variant="filled" onClick={handleVerify}>Verificar</Button>
          </Stack>
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
