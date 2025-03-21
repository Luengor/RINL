import { TbDeviceDesktopAnalytics, TbDeviceGamepad, TbLogout, TbUser, TbAB } from 'react-icons/tb';
import { Routes, Route, useLocation, useNavigate } from "react-router-dom";
import { useState, useEffect } from 'react';
import { AppShell, Button, Modal, Stack } from '@mantine/core';
import Media from "../../components/Media/Media";
import { Navbar } from "../../components/Navbar/Navbar";
import { logout } from "../../utils/session";
import Data from "./data";
import Stats from "./stats";

export default function My() {
  // Current page
  const location = useLocation();
  const [active, setActive] = useState("data");

  useEffect(() => {
    switch (location.pathname) {
      case '/my':
      case '/my/data':
        setActive('data');
        break;
      
      case '/my/stats':
        setActive('stats');
        break;
      
      case '/my/jugar':
        setActive('jugar');
        break;
      
      default:
        setActive('other');
        break;
    }
  }, [location, active]);

  // Logout
  const [logoutModal, setLogoutModal] = useState(false);
  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const navigate = useNavigate();
  return (
    <>
      <Modal opened={logoutModal} onClose={() => setLogoutModal(false)} title="Cerrar sesión" centered>
        <Stack>
          {`¿Estás seguro de que deseas cerrar sesión?`}
          <Button onClick={handleLogout} color="red">Cerrar sesión</Button>
        </Stack>
      </Modal>

      <AppShell
        navbar={{
          width: 100,
          breakpoint: '100px'
        }}
        padding="xl"
      >
      <AppShell.Navbar p="md">
        <Navbar 
          topLink={{ icon: TbAB, label: 'Dashboard', onClick: () => navigate('/') }}
          mainLinks={[
            { icon: TbUser, label: 'Cuenta', active: 'data' === active, onClick: () => navigate('/my/data') },
            { icon: TbDeviceDesktopAnalytics, active: 'stats' === active, label: 'Stats', onClick: () => navigate('/my/stats') },
            { icon: TbDeviceGamepad, label: 'Jugar', active: 'jugar' === active, onClick: () => navigate('/my/jugar') },
          ]}
          bottomLinks={[
            { icon: TbLogout, label: 'Salir', onClick: () => setLogoutModal(true) },
          ]}
        />
      </AppShell.Navbar>

      <AppShell.Main h="69px">
        <Routes>
          <Route index path="" element={<Data />} />
          <Route path="data" element={<Data />} />
          <Route path="stats" element={<Stats />} />
          <Route path="jugar" element={<Media />} />
          <Route path="*" element={<div>404</div>} />
        </Routes>
      </AppShell.Main>
    </AppShell>
    </>
  );
}
