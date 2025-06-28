import {
  Button,
  Collapse,
  Modal,
  NumberInput,
  Stack,
  Text,
  Title,
} from "@mantine/core";
import { useUser } from "../../hooks/useUser";
import { useDisclosure } from "@mantine/hooks";
import { Form, useForm } from "@mantine/form";
import { useClient } from "../../hooks/useClient";
import { createShapeShapePost, ShapeBase } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { OkNotification } from "../../utils/notifications";

interface FormValues {
  weight: number;
  height: number;
}

export default function CurrentShapeCard() {
  const { verified, latestShape, hasShape, refetch } = useUser();
  const [addShapeOpened, { open: openAddShape, close: closeAddshape }] =
    useDisclosure(false);

  const newShapeForm = useForm<FormValues>({
    name: "new-shape-form",
    mode: "uncontrolled",
    initialValues: {
      weight: latestShape?.weight || 0,
      height: latestShape?.height || 0,
    },
    validate: {
      weight: (value: number) =>
        value < 20 || value > 700 ? "Peso inválido" : null,
      height: (value: number) =>
        value < 50 || value > 300 ? "Altura inválida" : null,
    },
  });

  const queryClient = useQueryClient();
  const { client } = useClient();
  const addShapeMutation = useMutation({
    mutationKey: ["add-shape"],
    mutationFn: async (shape: ShapeBase) => {
      const req = await createShapeShapePost({
        client: client,
        body: shape,
      });
      return req;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["activity-shape-data"] });
      closeAddshape();
      OkNotification(
        "Forma física añadida",
        "La forma física ha sido añadida correctamente"
      );
      refetch();
    },

    meta: {
      errorMessage: "Error al añadir la forma física",
    },
  });

  async function handleAddShape() {
    const { weight, height } = newShapeForm.getValues();
    const shape: ShapeBase = {
      weight: weight,
      date: new Date().toISOString(),
      height: height,
    };

    addShapeMutation.mutate(shape);
  }

  let card_content = <Text>Loading...</Text>;
  if (!verified) {
    card_content = (
      <Text size="sm" c="dimmed">
        Tu cuenta no está verificada. Verifícala para poder añadir tu forma
        física.
      </Text>
    );
  } else {
    card_content = (
      <>
        <Collapse in={!hasShape}>
          <Text size="sm" c="dimmed">
            Todavía no has añadido ninguna forma física. Necesitas añadir tu
            forma física antes de jugar.
          </Text>
          <Button variant="outline" mt="sm" onClick={openAddShape}>
            Añadir forma física
          </Button>
        </Collapse>
        <Collapse in={hasShape}>
          <Text size="sm" c="dimmed">
            Última forma física registrada:{" "}
            {new Date(latestShape?.date).toLocaleDateString()}
          </Text>
          <Text size="sm" c="dimmed">
            Peso: {latestShape?.weight} kg
          </Text>
          <Text size="sm" c="dimmed">
            Altura: {latestShape?.height} cm
          </Text>
          <Button variant="outline" mt="sm" onClick={openAddShape}>
            Actualizar forma física
          </Button>
        </Collapse>
      </>
    );
  }

  return (
    <>
      <Title order={1} style={{ marginBottom: 0, lineHeight: 1 }}>
        Forma física actual
      </Title>

      {card_content}
      <Modal
        opened={addShapeOpened}
        onClose={closeAddshape}
        title="Añadir forma física"
        centered
      >
        <Form form={newShapeForm} onSubmit={handleAddShape}>
          <Stack align="stretch" gap="sm">
            <NumberInput
              label="Peso (kg)"
              key={newShapeForm.key("weight")}
              {...newShapeForm.getInputProps("weight")}
              placeholder="70"
            />

            <NumberInput
              label="Altura (cm)"
              key={newShapeForm.key("height")}
              {...newShapeForm.getInputProps("height")}
              placeholder="170"
            />
            <Button mt="lg" type="submit" variant="filled">
              Añadir forma
            </Button>
          </Stack>
        </Form>
      </Modal>
    </>
  );
}
