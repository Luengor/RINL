import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ModifyUser,
  updateMeUserMePut,
  UserBase,
  sendVerificationEmailUserVerifyEmailPost,
  deleteMeUserMeDelete,
} from "../../client";
import { useClient } from "../../hooks/useClient";
import { Form, hasLength, useForm } from "@mantine/form";
import {
  Stack,
  Text,
  TextInput,
  NumberInput,
  Chip,
  Collapse,
  Center,
  Button,
  Modal,
} from "@mantine/core";
import { TbUser, TbCalendar, TbMail } from "react-icons/tb";
import {
  OkNotification,
  ErrorNotification,
  WarningNotification,
} from "../../utils/notifications";
import { useDisclosure } from "@mantine/hooks";
import { useNavigate } from "react-router-dom";
import { useEffect } from "react";

interface DataFormValues {
  name: string;
  email: string;
  year_of_birth: number;
}

export default function DataForm({ user }: { user: UserBase }) {
  // Get the client
  const { client, logout } = useClient();
  const queryClient = useQueryClient();
  console.log("User data in DataForm:", user);

  // Delete things
  const [deleteModalOpened, { open, close }] = useDisclosure(false);
  const navigate = useNavigate();
  const handleDelete = async () => {
    try {
      await deleteMeUserMeDelete({ client: client });
      OkNotification(
        "Cuenta eliminada",
        "Tu cuenta ha sido eliminada correctamente."
      );
      queryClient.invalidateQueries({ queryKey: ["user-data"] });
      logout();
      navigate("/");
    } catch (error) {
      ErrorNotification(
        "Error al eliminar la cuenta",
        "No hemos podido eliminar tu cuenta. Por favor, inténtalo de nuevo más tarde."
      );
    } finally {
      close();
    }
  };

  // Send email mutation
  const sendEmailMutation = useMutation({
    mutationKey: ["send-verification-email"],
    mutationFn: async () => {
      await sendVerificationEmailUserVerifyEmailPost({ client: client });
    },
    onSuccess: () => {
      OkNotification(
        "Correo enviado",
        "Hemos enviado un correo electrónico de verificación a tu correo electrónico. Por favor, revisa tu bandeja de entrada."
      );
    },
    onError: () => {
      ErrorNotification(
        "Error al enviar el correo",
        "No hemos podido enviar el correo electrónico de verificación. Por favor, inténtalo de nuevo más tarde."
      );
    },
  });

  /// User data form (modify)
  const dataForm = useForm<DataFormValues>({
    name: "data-form",
    mode: "uncontrolled",
    initialValues: {
      name: user.name,
      email: user.email,
      year_of_birth: user.year_of_birth,
    },
    validate: {
      name: hasLength({ min: 1, max: 255 }, "Nombre no puede estar vacío"),
      email: hasLength({ min: 1, max: 255 }, "Correo no puede estar vacío"),
      year_of_birth: (value: number) =>
        value < 1900 || value >= new Date().getFullYear()
          ? "Año de nacimiento inválido"
          : null,
    },
  });

  // Update data form if user changes and its not dirty
  useEffect(() => {
    if (dataForm.isDirty()) return; // Don't update if form is dirty

    // Check if the values are the same
    if (
      dataForm.getValues().name === user.name &&
      dataForm.getValues().email === user.email &&
      dataForm.getValues().year_of_birth === user.year_of_birth
    ) {
      return; // No need to update if values are the same
    }

    // Update form values with user data
    const newValues = {
      name: user.name,
      email: user.email,
      year_of_birth: user.year_of_birth,
    };
    dataForm.setValues(newValues);
    dataForm.setInitialValues(newValues);
    dataForm.resetDirty();
  }, [dataForm, user]);

  // Update user data
  const updateUserMutation = useMutation({
    mutationKey: ["update-user"],
    mutationFn: async (data: ModifyUser) => {
      await updateMeUserMePut({ client: client, body: data });
    },

    onSuccess: () => {
      // Update form values
      const newValues = dataForm.getValues();
      const changedEmail = newValues.email !== user.email;
      newValues.email = user.email; // Reset email to original value
      dataForm.setValues(newValues);
      dataForm.setInitialValues(newValues);
      dataForm.resetDirty();

      // Show notification
      OkNotification(
        "Datos modificados",
        "Tus datos han sido modificados correctamente"
      );
      if (changedEmail) {
        WarningNotification(
          "Actualización de correo electrónico",
          "Para que el cambio de correo electrónico surta efecto, debes verificar la nueva dirección. Revisa tu bandeja de entrada para encontrar el enlace de verificación."
        );
      }

      queryClient.invalidateQueries({ queryKey: ["user-data"] });
    },

    onError: (err: any) => {
      const detail = (err.detail as string).toLowerCase();

      if (detail.includes("email")) {
        ErrorNotification(
          "El correo electrónico ya está en uso por otra cuenta. Por favor, utiliza otro correo electrónico.",
          "Error al modificar el correo electrónico"
        );

        dataForm.setFieldError(
          "email",
          "El correo electrónico ya está en uso por otra cuenta."
        );
      }
    },

    meta: { errorMessage: "No se pudo modificar tus datos." },
  });

  const handleModify = async () => {
    const { name, email, year_of_birth } = dataForm.getValues();
    updateUserMutation.mutate({
      email: email !== user.email ? (email as string) : null,
      name: name !== user.name ? (name as string) : null,
      year_of_birth: year_of_birth as number,
    });
  };

  return (
    <>
      <Form form={dataForm} onSubmit={handleModify}>
        <Stack align="stretch" gap="sm">
          <TextInput
            label="Nombre"
            key={dataForm.key("name")}
            {...dataForm.getInputProps("name")}
            leftSection={<TbUser />}
            placeholder="Nombre"
            readOnly={!user.verified}
          />
          <NumberInput
            label="Año de nacimiento"
            key={dataForm.key("year_of_birth")}
            {...dataForm.getInputProps("year_of_birth")}
            leftSection={<TbCalendar />}
            placeholder="2000"
            readOnly={!user.verified}
          />
          <Stack gap="0">
            <TextInput
              label="Correo electrónico"
              key={dataForm.key("email")}
              {...dataForm.getInputProps("email")}
              leftSection={<TbMail />}
              rightSection={
                <Chip readOnly checked={user.verified}>
                  {user.verified ? "Verificado" : "Sin verificar"}
                </Chip>
              }
              rightSectionWidth={user.verified ? 110 : 125}
              placeholder="ejemplo@ejemp.lo"
              readOnly={!user.verified}
            />
            <Collapse in={!user.verified}>
              <Text c="dimmed" size="sm" span>
                Tu correo electrónico no está verificado. Para verificarlo haz
                click en el enlace que te hemos enviado a tu correo electrónico.
                Si no lo has recibido, puedes reenviarlo haciendo click{" "}
              </Text>
              <Text
                style={{ cursor: "pointer" }}
                size="sm"
                span
                c="orange"
                onClick={() => sendEmailMutation.mutate()}
              >
                aquí.
              </Text>
            </Collapse>
          </Stack>
          <Collapse in={dataForm.isDirty()}>
            <Center>
              <Button
                loading={updateUserMutation.isPending}
                type="submit"
                w="100%"
              >
                Modificar datos
              </Button>
            </Center>
          </Collapse>

          <Button variant="light" color="red" onClick={() => open()}>
            Eliminar mi cuenta
          </Button>
        </Stack>
      </Form>
      <Modal
        opened={deleteModalOpened}
        onClose={close}
        title="Eliminar cuenta"
        centered
        size="lg"
      >
        <Stack>
          <Text>
            ¿Estás seguro de que quieres eliminar tu cuenta? Esta acción es
            irreversible y eliminará todos tus datos de forma permanente.
          </Text>
          <Text c="red">
            Esta acción no se puede deshacer. Asegúrate de que realmente quieres
            eliminar tu cuenta antes de continuar.
          </Text>
          <Center>
            <Button color="red" onClick={handleDelete}>
              Eliminar cuenta
            </Button>
          </Center>
        </Stack>
      </Modal>
    </>
  );
}
