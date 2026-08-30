import { AppShell, Burger, Group, Image, Stack, Text, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { SideNav } from '../components/SideNav';

export function DashboardPage() {
    const [opened, { toggle, close }] = useDisclosure();
    const [isCollapsed, setIsCollapsed] = useState(false);
    const currentYear = new Date().getFullYear();

    return (
        <AppShell
            header={{ height: 60 }}
            navbar={{
                width: isCollapsed ? 80 : 260,
                breakpoint: 'sm',
                collapsed: { mobile: !opened },
            }}
            padding="md"
            style={{ transition: 'all 200ms ease' }}
        >
            <AppShell.Header>
                <Group h="100%" px="md" justify="space-between" wrap="nowrap">
                    <Group wrap="nowrap" style={{ minWidth: 0 }}>
                        <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
                        <Title order={3} style={{ whiteSpace: 'nowrap' }}>
                            Fika WebApp
                        </Title>
                    </Group>

                    <Group gap="sm" wrap="nowrap" style={{ flexShrink: 0 }}>
                        <Stack gap={0} align="flex-end" visibleFrom="xs">
                            <Text fw={600} size="sm" style={{ fontFamily: 'bender' }}>
                                Fika Web Management
                            </Text>
                            <Text size="xs" c="dimmed">
                                © {currentYear} Project Fika. All rights reserved.
                            </Text>
                        </Stack>
                        <Image src="/images/FIKA_LOGO.png" h={32} w={32} style={{ flexShrink: 0 }} />
                    </Group>
                </Group>
            </AppShell.Header>

            <AppShell.Navbar p="sm">
                <SideNav collapsed={isCollapsed} onToggleCollapse={() => setIsCollapsed((prev) => !prev)} onLinkClick={close} />
            </AppShell.Navbar>

            <AppShell.Main>
                <Outlet />
            </AppShell.Main>
        </AppShell>
    );
}
