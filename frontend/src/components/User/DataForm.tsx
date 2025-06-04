import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ModifyUser,
  updateMeUserMePut,
  UserBase,
  verifyUserUserVerifyVerificationCodePost,
} from "../../client";
import { useClient } from "../../hooks/useClient";
import { useState } from "react";
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
  PinInput,
} from "@mantine/core";
import { TbUser, TbCalendar, TbMail } from "react-icons/tb";
import { OkNotification, ErrorNotification } from "../../utils/notifications";

interface DataFormValues {
  name: string;
  email: string;
  year_of_birth: number;
}

export default function DataForm({ user }: { user: UserBase }) {
  // Get the client
  const { client } = useClient();
  const queryClient = useQueryClient();

  /// Verify user
  const [verifing, setVerifing] = useState(false);
  const verifyForm = useForm({
    name: "verify-form",
    mode: "uncontrolled",
    validate: {
      pin: hasLength({ min: 6, max: 6 }, "El código debe tener 6 dígitos"),
    },
  });

  const verifyUserMutation = useMutation({
    mutationKey: ["verify-user"],
    mutationFn: async (pin: string) => {
      await verifyUserUserVerifyVerificationCodePost({
        client: client,
        path: { verification_code: pin },
      });
    },

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-data"] });
      setVerifing(false);
      OkNotification(
        "Correo verificado",
        "Tu correo electrónico ha sido verificado correctamente"
      );
    },

    onError: () => {
      ErrorNotification("No se ha podido verificar tu correo electrónico");
      verifyForm.setErrors({ pin: "Código incorrecto" });
    },
  });

  const handleVerify = async () => {
    const pin = (verifyForm.getValues().pin as string).toUpperCase();
    verifyUserMutation.mutate(pin);
  };

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
        value < 1900 || value > Date.now()
          ? "Año de nacimiento inválido"
          : null,
    },
  });

  // Update user data
  const updateUserMutation = useMutation({
    mutationKey: ["update-user"],
    mutationFn: async (data: ModifyUser) => {
      await updateMeUserMePut({ client: client, body: data });
    },

    onSuccess: () => {
      OkNotification(
        "Datos modificados",
        "Tus datos han sido modificados correctamente"
      );
      queryClient.invalidateQueries({ queryKey: ["user-data"] });

      // Update form values
      dataForm.setInitialValues(dataForm.getValues());
      dataForm.resetDirty();
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
                Verifica tu correo electrónico
              </Text>
              <Text
                style={{ cursor: "pointer" }}
                size="sm"
                span
                c="orange"
                onClick={() => setVerifing(true)}
              >
                {" aquí."}
              </Text>
            </Collapse>
          </Stack>
          <Collapse in={dataForm.isDirty()}>
            <Center>
              <Button loading={updateUserMutation.isPending} type="submit">
                Modificar datos
              </Button>
            </Center>
          </Collapse>
        </Stack>
      </Form>
      <Modal
        opened={verifing}
        title="Verificar correo electrónico"
        onClose={() => setVerifing(false)}
        centered
      >
        <Form form={verifyForm} onSubmit={handleVerify}>
          <Stack align="center">
            <Text>
              Introduce el código de verificación que te hemos enviado a{" "}
              {user.email}
            </Text>
            <PinInput
              name="pin"
              key={verifyForm.key("pin")}
              {...verifyForm.getInputProps("pin")}
              length={6}
              oneTimeCode
            />
            <Button
              loading={verifyUserMutation.isPending}
              mt="sm"
              type="submit"
              variant="filled"
            >
              Verificar
            </Button>
          </Stack>
        </Form>
      </Modal>
    </>
  );
}
