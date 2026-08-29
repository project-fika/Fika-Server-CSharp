import { Badge, Box, Button, Card, Group, Image, Loader, Modal, SimpleGrid, Stack, Text, Textarea, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconLogout, IconSend } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';

export const EFikaLocation = {
    None: 0,
    Hideout: 1,
    Customs: 2,
    Factory: 3,
    Interchange: 4,
    Labyrinth: 5,
    Lighthouse: 6,
    Reserve: 7,
    Streets: 8,
    Woods: 9,
    GroundZero: 10,
    Shoreline: 11,
    Laboratory: 12,
} as const;

export type EFikaLocation = (typeof EFikaLocation)[keyof typeof EFikaLocation];

export interface OnlinePlayer {
    profileId: string;
    nickname: string;
    level: number;
    location: EFikaLocation;
}

function getLocationName(location: EFikaLocation): string {
    const keys = Object.keys(EFikaLocation) as (keyof typeof EFikaLocation)[];
    return keys.find((key) => EFikaLocation[key] === location) || 'Unknown';
}

function getMapURL(location: EFikaLocation): string {
    switch (location) {
        case EFikaLocation.None:
            return '/images/tarkovlogo.jpg';
        case EFikaLocation.Hideout:
            return '/images/maps/hideout.png';
        case EFikaLocation.Factory:
            return '/images/maps/factory.png';
        case EFikaLocation.Customs:
            return '/images/maps/customs.png';
        case EFikaLocation.Woods:
            return '/images/maps/woods.png';
        case EFikaLocation.Shoreline:
            return '/images/maps/shoreline.png';
        case EFikaLocation.Interchange:
            return '/images/maps/customs.png';
        case EFikaLocation.Reserve:
            return '/images/maps/reserve.png';
        case EFikaLocation.Streets:
            return '/images/maps/streets.png';
        case EFikaLocation.Lighthouse:
            return '/images/maps/lighthouse.png';
        case EFikaLocation.GroundZero:
            return '/images/maps/groundzero.png';
        case EFikaLocation.Laboratory:
            return '/images/maps/labs.png';
        case EFikaLocation.Labyrinth:
            return '/images/maps/labyrinth.png';
        default:
            return '';
    }
}

function isRestricted(location: EFikaLocation): boolean {
    return location !== EFikaLocation.None && location !== EFikaLocation.Hideout;
}

export function PlayersPage() {
    const { hasRole } = useAuth();
    const queryClient = useQueryClient();

    const [targetPlayer, setTargetPlayer] = useState<OnlinePlayer | null>(null);
    const [messageText, setMessageText] = useState('');

    const { data: players = [], isLoading } = useQuery<OnlinePlayer[]>({
        queryKey: ['online-players'],
        queryFn: async () => {
            const res = await api.get<OnlinePlayer[]>('/players');
            if (res.data.length === 0) {
                notifications.show({ color: 'blue', message: 'No players online' });
            }
            return res.data;
        },
    });

    const logoutMutation = useMutation({
        mutationFn: (player: OnlinePlayer) => api.post('/players/logout', { profileId: player.profileId }),
        onSuccess: (_, player) => {
            notifications.show({ color: 'green', message: `Sent logout message to ${player.nickname}` });
            queryClient.invalidateQueries({ queryKey: ['online-players'] });
        },
        onError: (err: unknown) => {
            const message =
                err instanceof AxiosError ? err.response?.data?.message || 'Failed to send logout request' : 'Failed to send logout request';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    const sendMessageMutation = useMutation({
        mutationFn: ({ profileId, message }: { profileId: string; message: string }) => api.post('/players/send-message', { profileId, message }),
        onSuccess: () => {
            if (targetPlayer) {
                notifications.show({
                    color: 'green',
                    message: `Message sent to ${targetPlayer.nickname}`,
                });
            }
            setTargetPlayer(null);
            setMessageText('');
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to send message' : 'Failed to send message';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    const handleLogout = (player: OnlinePlayer) => {
        if (isRestricted(player.location)) {
            notifications.show({
                color: 'yellow',
                message: `${player.nickname} is in a raid and cannot be logged out`,
            });
            return;
        }
        logoutMutation.mutate(player);
    };

    if (isLoading) {
        return (
            <Stack align="center" justify="center" h={300}>
                <Loader size="lg" />
                <Text c="dimmed">Waiting for server...</Text>
            </Stack>
        );
    }

    const canManage = hasRole('Admin', 'Moderator');

    return (
        <Stack gap="md" style={{ width: '100%' }}>
            <Title order={2}>Online Players</Title>

            <SimpleGrid cols={{ base: 1, sm: 2, md: 3, xl: 4 }} spacing="lg">
                {players.map((player) => {
                    const restricted = isRestricted(player.location);
                    const locationName = getLocationName(player.location);

                    return (
                        <Card key={player.profileId} withBorder radius="md" p="0" style={{ overflow: 'hidden' }}>
                            <Card.Section>
                                <Image
                                    src={getMapURL(player.location)}
                                    height={160}
                                    alt={locationName}
                                    fallbackSrc="https://placehold.co/600x400?text=No+Map"
                                />
                            </Card.Section>

                            <Stack p="md" gap="xs" style={{ flexGrow: 1 }}>
                                <Group justify="space-between" align="center">
                                    <Text fw={700} size="md" c="#c7c5b3" truncate>
                                        {player.nickname}
                                    </Text>
                                    <Badge variant="default">Lvl {player.level}</Badge>
                                </Group>

                                <Text size="sm" c="dimmed">
                                    Location: <b>{locationName}</b>
                                </Text>

                                {canManage && (
                                    <Box
                                        mt="sm"
                                        p="xs"
                                        style={{
                                            backgroundColor: '#161616',
                                            border: '1px solid #333333',
                                            borderRadius: '16px',
                                        }}
                                    >
                                        <Group grow gap="xs">
                                            <Button
                                                size="xs"
                                                leftSection={<IconLogout size={14} />}
                                                disabled={restricted}
                                                loading={logoutMutation.isPending && logoutMutation.variables?.profileId === player.profileId}
                                                onClick={() => handleLogout(player)}
                                            >
                                                Logout
                                            </Button>
                                            <Button
                                                size="xs"
                                                leftSection={<IconSend size={14} />}
                                                disabled={restricted}
                                                onClick={() => setTargetPlayer(player)}
                                            >
                                                Message
                                            </Button>
                                        </Group>
                                    </Box>
                                )}
                            </Stack>
                        </Card>
                    );
                })}
            </SimpleGrid>

            <Modal opened={!!targetPlayer} onClose={() => setTargetPlayer(null)} title={`Send Message to ${targetPlayer?.nickname}`}>
                <Stack gap="sm">
                    <Textarea
                        label="Message"
                        placeholder="Type your message here..."
                        required
                        rows={4}
                        value={messageText}
                        onChange={(e) => setMessageText(e.currentTarget.value)}
                    />
                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={() => setTargetPlayer(null)}>
                            Cancel
                        </Button>
                        <Button
                            loading={sendMessageMutation.isPending}
                            disabled={!messageText.trim()}
                            onClick={() =>
                                targetPlayer &&
                                sendMessageMutation.mutate({
                                    profileId: targetPlayer.profileId,
                                    message: messageText,
                                })
                            }
                        >
                            Send
                        </Button>
                    </Group>
                </Stack>
            </Modal>
        </Stack>
    );
}
