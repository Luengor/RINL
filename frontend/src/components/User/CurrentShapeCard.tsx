import { Button, Modal, NumberInput, Paper, Slider, Stack, Text, Title, Tooltip } from "@mantine/core";
import { useUser } from "../../hooks/useUser";
import { useDisclosure } from "@mantine/hooks";
import { Form, useForm } from "@mantine/form";
import { useClient } from "../../hooks/useClient";
import { createShapeShapePost, ShapeBase } from "../../client";
import { useMutation } from "@tanstack/react-query";
import { OkNotification } from "../../utils/notifications";

export default function CurrentShapeCard() {
  const { verified, latestShape, hasShape, refetch } = useUser();
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
      weight: (value: number) => (value < 20 || value > 700 ? 'Peso inválido' : null),
      height: (value: number) => (value < 50 || value > 300 ? 'Altura inválida' : null),
      sex_math: (value: number) => (value < 0 || value > 1 ? 'Sexo inválido' : null),
    }
  });

  const { client } = useClient();
  const addShapeMutation = useMutation({
    mutationKey: ['add-shape'],
    mutationFn: async (shape: ShapeBase) => {
      const req = await createShapeShapePost({
        client: client,
        body: shape
      })
      return req;
    },
    onSuccess: () => {
      closeAddshape();
      OkNotification('Forma física añadida', 'La forma física ha sido añadida correctamente');
      refetch();
    },

    meta: {
      errorMessage: 'Error al añadir la forma física'
    },
  })

  async function handleAddShape() {
    const { weight, height, sex_math } = newShapeForm.getValues();
    const shape: ShapeBase = {
      weight: weight,
      date: new Date().toISOString(),
      height: height,
      sex_math: sex_math
    }

    addShapeMutation.mutate(shape);
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
        </>
      )}
      {hasShape && (
        <>
        <Text size="sm" c="dimmed">
          Última forma física registrada: {new Date(latestShape?.date).toLocaleDateString()}
        </Text>
        <Text size="sm" c="dimmed">
          Peso: {latestShape?.weight} kg
        </Text>
        <Text size="sm" c="dimmed">
          Altura: {latestShape?.height} cm
        </Text>
        </>
      )}
      <Button variant="outline" mt="sm" onClick={openAddShape}>
        Añadir forma
      </Button>
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
