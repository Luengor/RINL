import { Button, Loader, Modal, NumberInput, Paper, Slider, Stack, Text, Title, Tooltip } from "@mantine/core";
import { useUser } from "../../hooks/useUser";
import { useDisclosure } from "@mantine/hooks";
import { Form, useForm } from "@mantine/form";
import { useClient } from "../../hooks/useClient";
import { createShapeShapePost, ShapeBase } from "../../client";

export default function CurrentShapeCard() {
  const { verified, latestShape, latestShapeStatus, hasShape, refetch } = useUser();
  const [ addShapeOpened, { open: openAddShape, close: closeAddshape }] = useDisclosure(false);

  const newShapeForm = useForm({
    name: 'new-shape-form',
    mode: 'uncontrolled',
    initialValues: {
      weight: latestShape?.weight || 0,
      height: latestShape?.height || 0,
      sex_math: latestShape?.sex_math || 0.5,
    },
    validate: {
      weight: (value: number) => (value < 0 ? 'Peso inválido' : null),
      height: (value: number) => (value < 0 ? 'Altura inválida' : null),
      sex_math: (value: number) => (value < 0 || value > 1 ? 'Sexo inválido' : null),
    }
  });

  const { client } = useClient();
  async function handleAddShape() {
    const { weight, height, sex_math } = newShapeForm.getValues();
    const shape: ShapeBase = {
      weight: weight,
      date: new Date().toISOString(),
      height: height,
      sex_math: sex_math
    }

    console.log(shape, sex_math);

    const req = await createShapeShapePost({
      client: client,
      body: shape
    })

    if (req.response.ok) {
      closeAddshape();
      refetch();
    } else {
      newShapeForm.setErrors({ weight: 'Error al crear la forma' });
    }
  }

  return (
    <>
    <Paper shadow="md" p="sm">
      <Title order={3} fw="inherit">Forma actual</Title>

      {(verified && hasShape === false) && (
        <>
          <Title order={4}>Todavía no has introducido tu forma física</Title>
          <Text size="sm" c="dimmed">
            Necesitas introducir tu forma física para poder jugar.
          </Text>
          <Button variant="outline" mt="sm" onClick={openAddShape}>
            Introducir forma
          </Button>
        </>
      )}
      {hasShape && (
        <>
        </>
      )}
    </Paper>
    <Modal opened={addShapeOpened} onClose={closeAddshape} title="Añadir forma física" centered>
      <Form form={newShapeForm} onSubmit={handleAddShape}>
        <Stack align='stretch' gap="sm">
          <NumberInput
            label="Peso (kg)"
            key={newShapeForm.key('weight')}
            {...newShapeForm.getInputProps('weight')}
            placeholder='70' />

          <NumberInput
            label="Altura (cm)"
            key={newShapeForm.key('height')}
            {...newShapeForm.getInputProps('height')}
            placeholder='170' />

          <Tooltip label="A la hora de realizar cálculos de tu forma física, es necesario tener en cuenta el sexo.">
            <Text size="sm" fw={"bold"}>Sexo</Text>
          </Tooltip>
          <Slider
            key={newShapeForm.key('sex_math')}
            {...newShapeForm.getInputProps('sex_math')}

            min={0}
            max={1}
            step={0.01}
            marks={[
              { value: 0, label: 'Hombre' },
              { value: 1, label: 'Mujer' },
            ]}
            label={(value) => (value * 100).toFixed(0) + "%"}
            mx="xs"
            mb="xs"
            defaultValue={0.5}
            />

            <Button mt="lg" type="submit" variant="filled">Añadir forma</Button>
        </Stack>
      </Form>
    </Modal>
    </>
  ) 
}
