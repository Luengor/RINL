import {
  Grid,
  Loader,
  Stack,
  Text,
  Title
} from '@mantine/core';

import { useUser } from '../../hooks/useUser';
import DataForm from '../../components/User/DataForm';
import CurrentShapeCard from '../../components/User/CurrentShapeCard';

export default function Data() {
  // User Data
  const { user, userStatus } = useUser();

  if (userStatus === 'pending') {
    return <Loader type="dots" size="xl"/>
  } else if (userStatus === 'error') {
    return <Text>Error al cargar los datos</Text>
  }

  return (
    <>
    <Stack align='stretch'>
      <Title order={1}>Mis datos</Title>
      <Text >
        Aquí puedes ver y modificar tus datos.
      </Text>
      <Grid>
        <Grid.Col span={{ base: 12, lg: 6}}>
          <DataForm user={user} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, lg: 6}}>
          <CurrentShapeCard/>
        </Grid.Col>
      </Grid> 
    </Stack>
    </>
  )
}
