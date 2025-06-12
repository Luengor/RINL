import { notifications } from "@mantine/notifications";
import { TbCheck, TbX, TbExclamationMark } from "react-icons/tb";

export function OkNotification(title: string, message: string) {
  notifications.show({
    title: title,
    message: message,
    color: "green",
    icon: <TbCheck />,
  });
}

export function WarningNotification(title: string, message: string) {
  notifications.show({
    title: title,
    message: message,
    color: "yellow",
    icon: <TbExclamationMark />,
    withCloseButton: true,
    autoClose: false,
  });
}

export function ErrorNotification(message: string, title?: string | null) {
  notifications.show({
    title: title ?? "Error",
    message: message,
    color: "red",
    icon: <TbX />,
    withCloseButton: true,
    autoClose: false,
  });
}
