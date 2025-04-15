import { notifications } from "@mantine/notifications";
import { TbCheck, TbX } from "react-icons/tb";

export function OkNotification(title: string, message: string) {
  notifications.show({
    title: title,
    message: message,
    color: "green",
    icon: <TbCheck />,
  });
}

export function ErrorNotification(message: string, title?: string | null) {
  notifications.show({
    title: title ?? "Error",
    message: message,
    color: "red",
    icon: <TbX />,
  });
}
