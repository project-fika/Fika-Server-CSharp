import { Accordion, Box, Checkbox, Group, LoadingOverlay, Modal, Paper, ScrollArea, Stack, Text } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { useQuery } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useEffect, useMemo, useState } from 'react';
import { api } from '../api/axiosClient';
import type { ProfileResponse } from '../types/profiles';
import { type DetailedQuestData, EQuestState } from '../types/quests';

interface ProfileDetailedQuestsModalProps {
    profile: ProfileResponse | null;
    opened: boolean;
    onClose: () => void;
}

export function ProfileDetailedQuestsModal({ profile, opened, onClose }: ProfileDetailedQuestsModalProps) {
    const [renderedQuests, setRenderedQuests] = useState<DetailedQuestData[]>([]);
    const [isRendering, setIsRendering] = useState(false);

    const {
        data: quests = [],
        isLoading,
        isFetching,
        isError,
        error,
    } = useQuery<DetailedQuestData[]>({
        queryKey: ['profileDetailedQuests', profile?.profileId],
        queryFn: async () => {
            if (!profile?.profileId) return [];
            const res = await api.get<DetailedQuestData[]>(`/profiles/quests?profileId=${encodeURIComponent(profile.profileId)}`);
            return res.data;
        },
        enabled: !!profile?.profileId && opened,
    });

    useEffect(() => {
        if (isError) {
            const message = error instanceof AxiosError ? error.response?.data?.message || 'Failed to load detailed profile quests' : 'Failed to load detailed profile quests';
            notifications.show({
                color: 'red',
                message,
            });
        }
    }, [isError, error]);

    // Defer heavy DOM rendering when opening or when fresh data arrives
    useEffect(() => {
        if (opened && quests.length > 0) {
            setIsRendering(true);
            const timer = setTimeout(() => {
                setRenderedQuests(quests);
                setIsRendering(false);
            }, 50);

            return () => clearTimeout(timer);
        }

        if (opened && !isLoading && !isFetching && quests.length === 0) {
            setRenderedQuests([]);
            setIsRendering(false);
        }
    }, [quests, isLoading, isFetching, opened]);

    const handleClose = () => {
        setRenderedQuests([]);
        setIsRendering(false);
        onClose();
    };

    const activeQuests = useMemo(() => {
        return renderedQuests.filter((q) => !q.completed);
    }, [renderedQuests]);

    const completedQuests = useMemo(() => {
        return renderedQuests.filter((q) => q.completed);
    }, [renderedQuests]);

    const showLoader = isLoading || isRendering;

    const renderQuestAccordionList = (questList: DetailedQuestData[]) => {
        if (questList.length === 0) {
            return (
                <Text size="xs" c="dimmed" py="xs">
                    None found.
                </Text>
            );
        }

        return (
            <Accordion variant="separated" radius="sm">
                {questList.map((quest) => (
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
                                        <Accordion variant="subtle" radius="xs">
                                            <Accordion.Item value="objectives-list">
                                                <Accordion.Control p={0}>
                                                    <Text size="xs" fw={600}>
                                                        Objectives ({quest.objectives.length})
                                                    </Text>
                                                </Accordion.Control>
                                                <Accordion.Panel pt="xs">
                                                    <Stack gap="xs">
                                                        {quest.objectives.map((obj) => (
                                                            <Group key={`${quest.name}-${obj.description}`} align="flex-start" wrap="nowrap">
                                                                <Checkbox checked={obj.state === EQuestState.Completed} readOnly size="xs" mt={2} />
                                                                <Text size="xs">
                                                                    {obj.description}{' '}
                                                                    {obj.progress > 0 && (
                                                                        <Text span c="dimmed">
                                                                            ({obj.progress}/{obj.target})
                                                                        </Text>
                                                                    )}
                                                                </Text>
                                                            </Group>
                                                        ))}
                                                    </Stack>
                                                </Accordion.Panel>
                                            </Accordion.Item>
                                        </Accordion>
                                    </Paper>
                                )}
                            </Stack>
                        </Accordion.Panel>
                    </Accordion.Item>
                ))}
            </Accordion>
        );
    };

    return (
        <Modal opened={opened} onClose={handleClose} title={`Quests for ${profile?.nickname || 'Profile'}`} size="lg" centered>
            <Box pos="relative" mih={200}>
                <LoadingOverlay visible={showLoader} zIndex={1000} overlayProps={{ radius: 'sm', blur: 2 }} loaderProps={{ type: 'dots' }} />

                {!showLoader && renderedQuests.length === 0 ? (
                    <Text size="sm" c="dimmed" ta="center" py="xl">
                        No quests found for this profile.
                    </Text>
                ) : (
                    <ScrollArea.Autosize mah={550} type="auto">
                        <Accordion multiple defaultValue={['active', 'completed']} variant="filled" radius="md">
                            <Accordion.Item value="active">
                                <Accordion.Control>
                                    <Text fw={700} size="sm">
                                        Active ({activeQuests.length})
                                    </Text>
                                </Accordion.Control>
                                <Accordion.Panel>{renderQuestAccordionList(activeQuests)}</Accordion.Panel>
                            </Accordion.Item>

                            <Accordion.Item value="completed">
                                <Accordion.Control>
                                    <Text fw={700} size="sm">
                                        Completed ({completedQuests.length})
                                    </Text>
                                </Accordion.Control>
                                <Accordion.Panel>{renderQuestAccordionList(completedQuests)}</Accordion.Panel>
                            </Accordion.Item>
                        </Accordion>
                    </ScrollArea.Autosize>
                )}
            </Box>
        </Modal>
    );
}
