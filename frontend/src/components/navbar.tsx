import { useState } from 'react';
import {
  IconDeviceDesktopAnalytics,
  IconHome2,
  IconLogout,
  IconUser,
  IconDeviceGamepad
} from '@tabler/icons-react';
import { Stack, Tooltip, UnstyledButton } from '@mantine/core';
import classes from './navbar.module.css';

interface NavbarLinkProps {
  icon: typeof IconHome2;
  label: string;
  active?: boolean;
  onClick?: () => void;
}

function NavbarLink({ icon: Icon, label, active, onClick }: NavbarLinkProps) {
  return (
    <Tooltip label={label} position="right" transitionProps={{ duration: 0 }}>
      <UnstyledButton onClick={onClick} className={classes.link} data-active={active || undefined}>
        <Icon size={20} stroke={1.5} />
      </UnstyledButton>
    </Tooltip>
  );
}

const mockdata = [
  { icon: IconUser, label: 'Account' },
  { icon: IconDeviceGamepad, label: 'Home' },
  { icon: IconDeviceDesktopAnalytics, label: 'Analytics' },
];

interface NavbarProps {
  topLink: NavbarLinkProps;
  mainLinks: NavbarLinkProps[]; 
  bottomLinks: NavbarLinkProps[];
}

export function Navbar({topLink, mainLinks, bottomLinks} : NavbarProps) {
  const mlinks = mainLinks.map((link) => (
    <NavbarLink
      {...link}
      key={link.label}
      active={link.active}
      onClick={link.onClick}
    />
  ));

  const blinks = bottomLinks.map((link) => (
    <NavbarLink
      {...link}
      key={link.label}
      active={false}
      onClick={link.onClick}
    />
  ));

  return (
    <nav className={classes.navbar}>
      <div>
        <NavbarLink {...topLink}/>
      </div>

      <div className={classes.navbarMain}>
        <Stack justify="center" gap={0}>
          {mlinks}
        </Stack>
      </div>

      <Stack justify="center" gap={0}>
        {blinks}
      </Stack>
    </nav>
  );
}
