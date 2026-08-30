import { Accordion, ActionIcon, Button, Checkbox, Group, Loader, Modal, Paper, Stack, Table, Text, Title, Tooltip } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconTrash } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { api } from '../api/axiosClient';
import type { QueuedItemsResponse } from '../types/queuedItems';

interface QueuedItemsModalProps {
    opened: boolean;
    onClose: () => void;
}

export function QueuedItemsModal({ opened, onClose }: QueuedItemsModalProps) {
    const queryClient = useQueryClient();

    const { data, isLoading } = useQuery<QueuedItemsResponse>({
        queryKey: ['queued-items'],
        queryFn: async () => (await api.get<QueuedItemsResponse>('/tools/queued')).data,
        enabled: opened,
    });

    const deleteMutation = useMutation({
        mutationFn: (ticks: number) => api.post('/tools/queued/delete', { ticks }),
        onSuccess: () => {
            notifications.show({ color: 'green', message: 'Queued item delivery deleted' });
            queryClient.invalidateQueries({ queryKey: ['queued-items'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to delete timer' : 'Failed to delete timer';
            notifications.show({ color: 'red', message });
        },
    });

    return (
        <Modal opened={opened} onClose={onClose} title="Queued Items" size="xl" centered>
            {isLoading ? (
                <Stack align="center" py="xl">
                    <Loader size="md" />
                </Stack>
            ) : (
                <Stack gap="lg">
                    {/* Single Users */}
                    <Paper withBorder p="md">
                        <Title order={4} mb="sm">
                            To User
                        </Title>
                        {data?.singleTimers && data.singleTimers.length > 0 ? (
                            <Table verticalSpacing="xs">
                                <Table.Thead>
                                    <Table.Tr>
                                        <Table.Th>Profile ID</Table.Th>
                                        <Table.Th>Item</Table.Th>
                                        <Table.Th>Amount</Table.Th>
                                        <Table.Th>Message</Table.Th>
                                        <Table.Th style={{ textAlign: 'center' }}>Found In Raid</Table.Th>
                                        <Table.Th>Send Date</Table.Th>
                                        <Table.Th style={{ textAlign: 'right' }}>Action</Table.Th>
                                    </Table.Tr>
                                </Table.Thead>
                                <Table.Tbody>
                                    {data.singleTimers.map((item) => (
                                        <Table.Tr key={item.ticks}>
                                            <Table.Td>{item.profileId}</Table.Td>
                                            <Table.Td fw={600}>{item.itemName}</Table.Td>
                                            <Table.Td>{item.amount}</Table.Td>
                                            <Table.Td>{item.message}</Table.Td>
                                            <Table.Td style={{ textAlign: 'center' }}>
                                                <Checkbox checked={item.foundInRaid} readOnly style={{ display: 'inline-block' }} />
                                            </Table.Td>
                                            <Table.Td>{new Date(item.sendDate).toLocaleString()}</Table.Td>
                                            <Table.Td style={{ textAlign: 'right' }}>
                                                <Tooltip label="Delete queued item(s)">
                                                    <ActionIcon color="red" variant="filled" onClick={() => deleteMutation.mutate(item.ticks)}>
                                                        <IconTrash size={16} />
                                                    </ActionIcon>
                                                </Tooltip>
                                            </Table.Td>
                                        </Table.Tr>
                                    ))}
                                </Table.Tbody>
                            </Table>
                        ) : (
                            <Text c="dimmed" size="sm">
                                There are no queued item deliveries to single users
                            </Text>
                        )}
                    </Paper>

                    {/* To Everyone */}
                    <Paper withBorder p="md">
                        <Title order={4} mb="sm">
                            To Everyone
                        </Title>
                        {data?.allTimers && data.allTimers.length > 0 ? (
                            <Accordion variant="separated">
                                {data.allTimers.map((item) => (
                                    <Accordion.Item key={item.ticks} value={item.ticks.toString()}>
                                        <Accordion.Control>
                                            <Group justify="space-between">
                                                <Text fw={600}>
                                                    {item.itemName} (x{item.amount})
                                                </Text>
                                                <Text size="xs" c="dimmed">
                                                    {new Date(item.sendDate).toLocaleString()}
                                                </Text>
                                            </Group>
                                        </Accordion.Control>
                                        <Accordion.Panel>
                                            <Stack gap="xs">
                                                <Text size="sm">
                                                    <b>Message:</b> {item.message || 'None'}
                                                </Text>
                                                <Text size="xs" c="dimmed">
                                                    <b>Target Recipients ({item.profileIds?.length || 0}):</b> {item.profileIds?.join(', ')}
                                                </Text>
                                                <Group justify="flex-end" mt="xs">
                                                    <Button
                                                        color="red"
                                                        size="xs"
                                                        leftSection={<IconTrash size={14} />}
                                                        onClick={() => deleteMutation.mutate(item.ticks)}
                                                    >
                                                        Delete Delivery
                                                    </Button>
                                                </Group>
                                            </Stack>
                                        </Accordion.Panel>
                                    </Accordion.Item>
                                ))}
                            </Accordion>
                        ) : (
                            <Text c="dimmed" size="sm">
                                There are no queued item deliveries to all users
                            </Text>
                        )}
                    </Paper>
                </Stack>
            )}
        </Modal>
    );
}
