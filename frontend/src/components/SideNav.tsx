import { ActionIcon, Box, Divider, NavLink, Stack, Tooltip } from '@mantine/core';
import {
    IconBrandGithub,
    IconChartPie,
    IconChevronLeft,
    IconChevronRight,
    IconFolderShare,
    IconHome,
    IconInfoCircle,
    IconLock,
    IconLogout,
    IconSettings,
    IconShield,
    IconTools,
    IconUser,
    IconUsers,
    IconUsersGroup,
} from '@tabler/icons-react';
import { useState } from 'react';
import { NavLink as RouterNavLink, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ChangePasswordModal } from './ChangePasswordModal';

interface SideNavProps {
    collapsed: boolean;
    onToggleCollapse: () => void;
    onLinkClick?: () => void;
}

export function SideNav({ collapsed, onToggleCollapse, onLinkClick }: SideNavProps) {
    const { user, logout, hasRole } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [passwordModalOpened, setPasswordModalOpened] = useState(false);

    const handleLogout = () => {
        logout();
        navigate('/');
        onLinkClick?.();
    };

    const handleOpenPasswordModal = () => {
        setPasswordModalOpened(true);
        onLinkClick?.();
    };

    const renderLink = (label: string, icon: React.ReactNode, to?: string, disabled?: boolean, onClick?: () => void, external?: boolean) => {
        const isActive = to ? location.pathname === to : false;

        const navLinkElement = external ? (
            <NavLink
                component="a"
                href={to}
                target="_blank"
                rel="noopener noreferrer"
                label={collapsed ? null : label}
                leftSection={icon}
                disabled={disabled}
                onClick={onClick}
            />
        ) : to ? (
            <NavLink
                component={RouterNavLink}
                to={to}
                label={collapsed ? null : label}
                leftSection={icon}
                active={isActive}
                disabled={disabled}
                onClick={onClick}
            />
        ) : (
            <NavLink label={collapsed ? null : label} leftSection={icon} disabled={disabled} onClick={onClick} />
        );

        if (collapsed) {
            return (
                <Tooltip label={label} position="right" withArrow key={label}>
                    <Box>{navLinkElement}</Box>
                </Tooltip>
            );
        }

        return navLinkElement;
    };

    return (
        <Box h="100%" display="flex" style={{ flexDirection: 'column' }}>
            <Box display="flex" style={{ justifyContent: collapsed ? 'center' : 'flex-end' }} mb="xs">
                <ActionIcon variant="subtle" color="gray" onClick={onToggleCollapse}>
                    {collapsed ? <IconChevronRight size={18} /> : <IconChevronLeft size={18} />}
                </ActionIcon>
            </Box>

            <Stack gap="xs" style={{ flexGrow: 1, overflowY: 'auto' }}>
                {renderLink('Home', <IconHome size={18} stroke={1.5} />, '/dashboard', false, onLinkClick)}

                {hasRole('Admin') && (
                    <>
                        {renderLink('Accounts', <IconShield size={18} stroke={1.5} />, '/fika/manage/accounts', false, onLinkClick)}
                        {collapsed ? (
                            renderLink('Configuration', <IconSettings size={18} stroke={1.5} />, undefined, true)
                        ) : (
                            <Tooltip label="Not yet implemented" position="right" key="Configuration">
                                <Box>
                                    <NavLink disabled label="Configuration" leftSection={<IconSettings size={18} stroke={1.5} />} />
                                </Box>
                            </Tooltip>
                        )}
                    </>
                )}

                {hasRole('Admin', 'Moderator') && (
                    <>
                        {renderLink('Profiles', <IconUser size={18} stroke={1.5} />, '/fika/manage/profiles', false, onLinkClick)}
                        {renderLink('Tools', <IconTools size={18} stroke={1.5} />, '/fika/manage/tools', false, onLinkClick)}
                    </>
                )}

                {renderLink('Players', <IconUsers size={18} stroke={1.5} />, '/fika/manage/players', false, onLinkClick)}
                {renderLink('Statistics', <IconChartPie size={18} stroke={1.5} />, '/fika/manage/statistics', false, onLinkClick)}
                {renderLink('Headless', <IconUsersGroup size={18} stroke={1.5} />, '/fika/manage/headless', false, onLinkClick)}
                {renderLink('File Manager', <IconFolderShare size={18} stroke={1.5} />, '/fika/manage/filemanager', false, onLinkClick)}
            </Stack>

            <Divider my="sm" />

            <Stack gap="xs">
                {user && (
                    <>
                        {renderLink('Change Password', <IconLock size={18} stroke={1.5} />, undefined, false, handleOpenPasswordModal)}
                        {renderLink(
                            'Logout',
                            <IconLogout size={18} stroke={1.5} color="var(--mantine-color-red-4)" />,
                            undefined,
                            false,
                            handleLogout,
                        )}
                    </>
                )}

                {renderLink('About', <IconInfoCircle size={18} stroke={1.5} />, '/about', false, onLinkClick)}
                {renderLink(
                    'GitHub',
                    <IconBrandGithub size={18} stroke={1.5} />,
                    'https://github.com/project-fika/fika-server-csharp',
                    false,
                    onLinkClick,
                    true,
                )}
            </Stack>

            <ChangePasswordModal opened={passwordModalOpened} onClose={() => setPasswordModalOpened(false)} />
        </Box>
    );
}
