import { Accordion, Box, LoadingOverlay, Modal, Paper, ScrollArea, Stack, Text } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { useQuery } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useEffect, useState } from 'react';
import { api } from '../api/axiosClient';
import type { ProfileResponse } from '../types/profiles';
import type { QuestData } from '../types/quests';

interface ProfileQuestsModalProps {
    profile: ProfileResponse | null;
    onClose: () => void;
}

export function ProfileQuestsModal({ profile, onClose }: ProfileQuestsModalProps) {
    const [renderedQuests, setRenderedQuests] = useState<QuestData[]>([]);
    const [isRendering, setIsRendering] = useState(false);

    const {
        data: quests = [],
        isLoading,
        isError,
        error,
    } = useQuery<QuestData[]>({
        queryKey: ['profileQuests', profile?.profileId],
        queryFn: async () => {
            if (!profile?.profileId) return [];
            const res = await api.get<QuestData[]>(`/profiles/quests?profileId=${encodeURIComponent(profile.profileId)}`);
            return res.data;
        },
        enabled: !!profile?.profileId,
    });

    useEffect(() => {
        if (isError) {
            const message = error instanceof AxiosError ? error.response?.data?.message || 'Failed to load profile quests' : 'Failed to load profile quests';
            notifications.show({
                color: 'red',
                message,
            });
        }
    }, [isError, error]);

    useEffect(() => {
        if (!isLoading && quests.length > 0) {
            setIsRendering(true);
            const timer = setTimeout(() => {
                setRenderedQuests(quests);
                setIsRendering(false);
            }, 50);

            return () => clearTimeout(timer);
        }

        if (!isLoading && quests.length === 0) {
            setRenderedQuests([]);
            setIsRendering(false);
        }
    }, [quests, isLoading]);

    const handleClose = () => {
        setRenderedQuests([]);
        setIsRendering(false);
        onClose();
    };

    const showLoader = isLoading || isRendering;

    return (
        <Modal opened={!!profile} onClose={handleClose} title={`Quests for ${profile?.nickname || 'Profile'}`} size="lg" centered>
            <Box pos="relative" mih={200}>
                <LoadingOverlay visible={showLoader} zIndex={1000} overlayProps={{ radius: 'sm', blur: 2 }} loaderProps={{ type: 'dots' }} />

                {!showLoader && renderedQuests.length === 0 ? (
                    <Text size="sm" c="dimmed" ta="center" py="xl">
                        No active quests found for this profile.
                    </Text>
                ) : (
                    <ScrollArea.Autosize mah={500} type="auto">
                        <Accordion variant="separated" radius="md">
                            {renderedQuests.map((quest) => (
                                <Accordion.Item key={quest.name} value={quest.name}>
                                    <Accordion.Control>
                                        <Text fw={600} size="sm">
                                            {quest.name}
                                        </Text>
                                    </Accordion.Control>
                                    <Accordion.Panel>
                                        <Stack gap="xs">
                                            <Text size="sm" c="dimmed" style={{ whiteSpace: 'pre-line' }}>
                                                {quest.description || 'No description available.'}
                                            </Text>

                                            {quest.objectives && quest.objectives.length > 0 && (
                                                <Paper withBorder p="xs" radius="sm" mt="xs" style={{ backgroundColor: 'var(--mantine-color-dark-6)' }}>
                                                    <Stack gap={4}>
                                                        <Text size="xs" fw={600}>
                                                            Objectives ({quest.objectives.length}):
                                                        </Text>
                                                        {quest.objectives.map((obj) => (
                                                            <Text key={`${quest.name}-${obj.description}`} size="xs">
                                                                • {obj.description}
                                                            </Text>
                                                        ))}
                                                    </Stack>
                                                </Paper>
                                            )}
                                        </Stack>
                                    </Accordion.Panel>
                                </Accordion.Item>
                            ))}
                        </Accordion>
                    </ScrollArea.Autosize>
                )}
            </Box>
        </Modal>
    );
}
