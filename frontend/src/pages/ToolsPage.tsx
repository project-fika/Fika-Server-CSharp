import { Button, Card, Group, SimpleGrid, Stack, Text, ThemeIcon, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { IconListDetails, IconMailForward, IconRefresh, IconSearch, IconTools } from '@tabler/icons-react';
import { useMutation } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import { QuestSearchModal } from '../components/QuestSearchModal';
import { QueuedItemsModal } from '../components/QueuedItemsModal';
import { SendItemModal } from '../components/SendItemModal';
import type { SendItemModel } from '../types/items';
import type { ProfileResponse } from '../types/profiles';

export function ToolsPage() {
    const [sendModalOpened, setSendModalOpened] = useState(false);
    const [queuedModalOpened, setQueuedModalOpened] = useState(false);
    const [questModalOpened, { open: openQuestModal, close: closeQuestModal }] = useDisclosure(false);

    const refreshDbMutation = useMutation({
        mutationFn: () => api.post('/tools/items/refresh'),
        onSuccess: () => {
            notifications.show({ color: 'green', message: 'Items successfully refreshed' });
        },
        onError: () => {
            notifications.show({ color: 'red', message: 'There was an error refreshing the database' });
        },
    });

    const sendToAllMutation = useMutation({
        mutationFn: async (model: SendItemModel) => {
            if (model.date && model.date < new Date()) {
                throw new Error('You cannot send items to the past!');
            }

            const profilesRes = await api.get<ProfileResponse[]>('/profiles');
            const profileIds = profilesRes.data.map((p) => p.profileId);

            const requestPayload = {
                profileIds,
                itemTemplate: model.templateId,
                itemTpl: model.templateId,
                amount: model.amount,
                message: model.message,
                foundInRaid: model.foundInRaid,
                fir: model.foundInRaid,
                expirationDays: model.expirationDays,
            };

            if (model.useDate && model.date) {
                return api.post('/tools/schedule/all', {
                    request: requestPayload,
                    sendDate: model.date.toISOString(),
                });
            }

            return api.post('/tools/senditemtoall', requestPayload);
        },
        onSuccess: (_, variables) => {
            setSendModalOpened(false);
            if (variables.useDate && variables.date) {
                notifications.show({
                    color: 'green',
                    message: `The item was queued to be sent to everyone at ${variables.date.toLocaleString()}.`,
                });
            } else {
                notifications.show({ color: 'green', message: 'The item was sent to everyone.' });
            }
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || err.message : err instanceof Error ? err.message : 'Error sending item to everyone';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    return (
        <Stack gap="lg" style={{ width: '100%' }}>
            <Group justify="space-between" align="center">
                <Group gap="xs">
                    <ThemeIcon size="lg" radius="md">
                        <IconTools size={20} />
                    </ThemeIcon>
                    <Title order={2}>Server Tools</Title>
                </Group>
            </Group>

            <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="md">
                <Card withBorder padding="lg" radius="md">
                    <Stack justify="space-between" h="100%" gap="md">
                        <div>
                            <Text fw={600} size="lg" mb={4}>
                                Global Broadcast
                            </Text>
                            <Text size="sm" c="dimmed">
                                Dispatch items instantly or schedule deliveries to all active profiles.
                            </Text>
                        </div>
                        <Button fullWidth leftSection={<IconMailForward size={16} />} onClick={() => setSendModalOpened(true)}>
                            Send To Everyone
                        </Button>
                    </Stack>
                </Card>

                <Card withBorder padding="lg" radius="md">
                    <Stack justify="space-between" h="100%" gap="md">
                        <div>
                            <Text fw={600} size="lg" mb={4}>
                                Delivery Queue
                            </Text>
                            <Text size="sm" c="dimmed">
                                Inspect, monitor, and manage pending or scheduled item deliveries.
                            </Text>
                        </div>
                        <Button fullWidth leftSection={<IconListDetails size={16} />} onClick={() => setQueuedModalOpened(true)}>
                            View Queued Items
                        </Button>
                    </Stack>
                </Card>

                <Card withBorder padding="lg" radius="md">
                    <Stack justify="space-between" h="100%" gap="md">
                        <div>
                            <Text fw={600} size="lg" mb={4}>
                                Database Sync
                            </Text>
                            <Text size="sm" c="dimmed">
                                Force-refresh the local item and quests templates.
                            </Text>
                        </div>
                        <Button fullWidth leftSection={<IconRefresh size={16} />} loading={refreshDbMutation.isPending} onClick={() => refreshDbMutation.mutate()}>
                            Refresh Database
                        </Button>
                    </Stack>
                </Card>

                <Card withBorder padding="lg" radius="md">
                    <Stack justify="space-between" h="100%" gap="md">
                        <div>
                            <Text fw={600} size="lg" mb={4}>
                                Quest Lookup
                            </Text>
                            <Text size="sm" c="dimmed">
                                Search for quests in the database and view their objectives.
                            </Text>
                        </div>
                        <Button fullWidth leftSection={<IconSearch size={16} />} onClick={openQuestModal}>
                            Search Quests
                        </Button>
                    </Stack>
                </Card>
            </SimpleGrid>

            <SendItemModal opened={sendModalOpened} onClose={() => setSendModalOpened(false)} loading={sendToAllMutation.isPending} onConfirm={(model) => sendToAllMutation.mutate(model)} />

            <QuestSearchModal opened={questModalOpened} onClose={closeQuestModal} />

            <QueuedItemsModal opened={queuedModalOpened} onClose={() => setQueuedModalOpened(false)} />
        </Stack>
    );
}
