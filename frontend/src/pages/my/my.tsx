import {
  TbDeviceDesktopAnalytics,
  TbDeviceGamepad,
  TbLogout,
  TbUser,
  TbHome
} from "react-icons/tb";
import { Routes, Route, useLocation, useNavigate } from "react-router-dom";
import { useState, useEffect, useRef } from "react";
import { AppShell, Button, Modal, Stack } from "@mantine/core";
import Media from "../../components/Media/Media";
import { Navbar } from "../../components/Navbar/Navbar";
import Data from "./data";
import Stats from "./stats";
import { useClient } from "../../hooks/useClient";
import { useUser } from "../../hooks/useUser";
import { NotFoundPage } from "../../components/NotFound/NotFound";
import { verifyEmailTokenUserMeEmailTokenPost } from "../../client";
import { ErrorNotification, OkNotification } from "../../utils/notifications";

export default function My() {
  // Get the client
  const { logout, client } = useClient();

  // Get user data
  const { user, userStatus, verified, hasShape, refetch: user_refetch } = useUser();

  // Current page
  const location = useLocation();
  const [active, setActive] = useState("data");

  const navigate = useNavigate();
  useEffect(() => {
    let active;

    switch (location.pathname) {
      case "/my":
      case "/my/data":
        active = "data";
        break;

      case "/my/stats":
        active = "stats";
        break;

      case "/my/game":
        active = "game";
        break;

      default:
        active = "other";
        break;
    }

    if (
      userStatus === "success" &&
      user.verified === false &&
      active !== "data"
    ) {
      navigate("/my/data");
    }
    setActive(active);
  }, [location, active, user, userStatus, navigate]);

  // Check for a verify=token in the URL to verify an email
  const verified_tokens = useRef<string[]>([]);
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get("verify");

    // If no token, do nothing
    if (!token || verified_tokens.current.includes(token)) {
      return;
    }

    verified_tokens.current.push(token);
    console.log("Verifying email with token:", token, verified_tokens.current);

    // Verify the email if the token is present
    const verifyEmail = async () => {
      try {
        await verifyEmailTokenUserMeEmailTokenPost({
          client: client,
          query: { token: token },
        })

        OkNotification(
          "Correo electrónico verificado",
          "Tu correo electrónico ha sido verificado correctamente."
        );

        console.log("Email verified successfully");
        user_refetch();

      } catch (error) {
        ErrorNotification(
          "Error al verificar el correo electrónico",
          "El token de verificación no es válido o ha expirado."
        )
        console.error("Error verifying email:", error);
      }

    }

    verifyEmail()

    // Remove the token from the URL
    params.delete("verify");
    navigate({
      pathname: location.pathname,
      search: params.toString(),
    });

  }, [client, location.pathname, location.search, navigate]);

  // Logout
  const [logoutModal, setLogoutModal] = useState(false);
  const handleLogout = () => {
    logout();
    navigate("/");
  };

  // Links
  const links = [
    {
      icon: TbUser,
      label: "Cuenta",
      active: "data" === active,
      onClick: () => navigate("/my/data"),
    },
    {
      icon: TbDeviceDesktopAnalytics,
      active: "stats" === active,
      disabled: !verified,
      label: "Estadísticas",
      onClick: () => navigate("/my/stats"),
    },
    {
      icon: TbDeviceGamepad,
      label: "Juego",
      active: "game" === active,
      disabled: !verified || !hasShape,
      onClick: () => navigate("/my/game"),
    },
  ];

  // Check if the user is on mobile to move the navbar to the top
  const isMobile = window.innerWidth <= window.innerHeight;
  let navbar;
  if (isMobile) {
    navbar = (
      <AppShell.Header p="md">
        <Navbar
          row={true}
          topLink={{
            icon: TbHome,
            label: "Dashboard",
            onClick: () => navigate("/"),
          }}
          mainLinks={links}
          bottomLinks={[
            {
              icon: TbLogout,
              label: "Salir",
              onClick: () => setLogoutModal(true),
            },
          ]}
        />
      </AppShell.Header>
    );
  } else {
    navbar = (
      <AppShell.Navbar p="md">
        <Navbar
          row={false}
          topLink={{
            icon: TbHome,
            label: "Dashboard",
            onClick: () => navigate("/"),
          }}
          mainLinks={links}
          bottomLinks={[
            {
              icon: TbLogout,
              label: "Salir",
              onClick: () => setLogoutModal(true),
            },
          ]}
        />
      </AppShell.Navbar>
    );
  }

  return (
    <>
      <Modal
        opened={logoutModal}
        onClose={() => setLogoutModal(false)}
        title="Cerrar sesión"
        centered
      >
        <Stack>
          {`¿Estás seguro de que deseas cerrar sesión?`}
          <Button onClick={handleLogout} color="red">
            Cerrar sesión
          </Button>
        </Stack>
      </Modal>

      <AppShell
        navbar={{
          width: 80,
          breakpoint: "80px",
          collapsed: {
            mobile: true,
            desktop: isMobile,
          }
        }}
        header={{
          height: 60,
          collapsed: !isMobile,
        }}
        padding="xl"
      >
        {navbar}

        <AppShell.Main h="69px">
          <Routes>
            <Route index path="" element={<Data />} />
            <Route path="data" element={<Data />} />
            <Route path="stats" element={<Stats />} />
            <Route path="game" element={<Media />} />
            <Route path="*" element={<NotFoundPage/>} />
          </Routes>
        </AppShell.Main>
      </AppShell>
    </>
  );
}
