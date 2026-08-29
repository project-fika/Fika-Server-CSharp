import { Badge, Box, Button, Card, Group, Loader, Paper, Stack, Table, Text, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconRotateClockwise } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { api } from '../api/axiosClient';

export const EHeadlessState = {
    Ready: 0,
    NotReady: 1,
} as const;

export type EHeadlessState = (typeof EHeadlessState)[keyof typeof EHeadlessState];

export interface OnlineHeadless {
    profileId: string;
    nickname: string;
    state: EHeadlessState;
    players: number;
}

export function HeadlessPage() {
    const queryClient = useQueryClient();

    // Fetch Clients
    const { data: clients = [], isLoading } = useQuery<OnlineHeadless[]>({
        queryKey: ['headless-clients'],
        queryFn: async () => (await api.get<OnlineHeadless[]>('/headless')).data,
    });

    // Restart Mutation
    const restartMutation = useMutation({
        mutationFn: (profileId: string) => api.post('/headless/restart', { profileId }),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data.message || 'Restart initiated' });
            queryClient.invalidateQueries({ queryKey: ['headless-clients'] });
        },
        onError: (err: unknown) => {
            const message =
                err instanceof AxiosError ? err.response?.data?.message || 'Error executing restart request' : 'Error executing restart request';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    const renderStateBadge = (state: EHeadlessState) => {
        return state === EHeadlessState.Ready ? (
            <Badge color="green" variant="light">
                Ready
            </Badge>
        ) : (
            <Badge color="red" variant="light">
                Not Ready
            </Badge>
        );
    };

    if (isLoading) {
        return (
            <Stack align="center" justify="center" h={300}>
                <Loader size="lg" />
                <Text c="dimmed">Waiting for server...</Text>
            </Stack>
        );
    }

    return (
        <Stack gap="md">
            <Title order={2}>Headless Clients</Title>

            <Box visibleFrom="sm">
                <Paper withBorder p="md">
                    <Table verticalSpacing="sm" highlightOnHover>
                        <Table.Thead>
                            <Table.Tr>
                                <Table.Th>Profile ID</Table.Th>
                                <Table.Th>Nickname</Table.Th>
                                <Table.Th>State</Table.Th>
                                <Table.Th>Players</Table.Th>
                                <Table.Th style={{ textAlign: 'right' }}>Actions</Table.Th>
                            </Table.Tr>
                        </Table.Thead>
                        <Table.Tbody>
                            {clients.map((item) => (
                                <Table.Tr key={item.profileId}>
                                    <Table.Td>{item.profileId}</Table.Td>
                                    <Table.Td>{item.nickname}</Table.Td>
                                    <Table.Td>{renderStateBadge(item.state)}</Table.Td>
                                    <Table.Td>{item.players}</Table.Td>
                                    <Table.Td style={{ textAlign: 'right' }}>
                                        <Button
                                            size="xs"
                                            leftSection={<IconRotateClockwise size={14} />}
                                            disabled={item.state === EHeadlessState.NotReady}
                                            loading={restartMutation.isPending && restartMutation.variables === item.profileId}
                                            onClick={() => restartMutation.mutate(item.profileId)}
                                        >
                                            Restart
                                        </Button>
                                    </Table.Td>
                                </Table.Tr>
                            ))}
                        </Table.Tbody>
                    </Table>
                </Paper>
            </Box>

            <Box hiddenFrom="sm">
                <Stack gap="sm">
                    {clients.map((item) => (
                        <Card key={item.profileId} withBorder radius="md" p="md">
                            <Group justify="space-between" mb="xs">
                                <Text fw={700} size="md">
                                    {item.nickname}
                                </Text>
                                {renderStateBadge(item.state)}
                            </Group>

                            <Text size="xs" c="dimmed" mb={2}>
                                Profile ID: {item.profileId}
                            </Text>
                            <Text size="sm" mb="md">
                                Active Players: <b>{item.players}</b>
                            </Text>

                            <Button
                                fullWidth
                                size="sm"
                                leftSection={<IconRotateClockwise size={16} />}
                                disabled={item.state === EHeadlessState.NotReady}
                                loading={restartMutation.isPending && restartMutation.variables === item.profileId}
                                onClick={() => restartMutation.mutate(item.profileId)}
                            >
                                Restart Client
                            </Button>
                        </Card>
                    ))}
                </Stack>
            </Box>
        </Stack>
    );
}
