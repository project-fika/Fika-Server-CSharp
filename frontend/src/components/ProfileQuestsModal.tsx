import { Accordion, ActionIcon, Box, Button, Group, LoadingOverlay, Modal, Paper, ScrollArea, Stack, Text, Tooltip } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconCheck } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useEffect, useState } from 'react';
import { api } from '../api/axiosClient';
import type { ProfileResponse } from '../types/profiles';
import { EQuestState, type QuestData } from '../types/quests';

interface ProfileQuestsModalProps {
    profile: ProfileResponse | null;
    onClose: () => void;
}

interface CompleteQuestPayload {
    questId: string;
    objectiveId?: string;
}

export function ProfileQuestsModal({ profile, onClose }: ProfileQuestsModalProps) {
    const queryClient = useQueryClient();
    const [renderedQuests, setRenderedQuests] = useState<QuestData[]>([]);
    const [isRendering, setIsRendering] = useState(false);

    const {
        data: quests = [],
        isLoading,
        isFetching,
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

    const completeMutation = useMutation({
        mutationFn: async ({ questId, objectiveId }: CompleteQuestPayload) => {
            if (!profile?.profileId) return;

            let url = `/profiles/quests/complete?profileId=${encodeURIComponent(profile.profileId)}&questId=${encodeURIComponent(questId)}`;
            if (objectiveId) {
                url += `&objectiveId=${encodeURIComponent(objectiveId)}`;
            }

            const res = await api.post(url);
            return res.data;
        },
        onSuccess: (data, variables) => {
            const message = data?.message || (variables.objectiveId ? 'Objective completed' : 'Quest completed');
            notifications.show({ color: 'green', message });
            queryClient.invalidateQueries({ queryKey: ['profileQuests', profile?.profileId] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to complete quest action' : 'Failed to complete quest action';
            notifications.show({ color: 'red', message });
        },
    });

    useEffect(() => {
        if (isError) {
            const message = error instanceof AxiosError ? error.response?.data?.message || 'Failed to load profile quests' : 'Failed to load profile quests';
            notifications.show({ color: 'red', message });
        }
    }, [isError, error]);

    useEffect(() => {
        if (quests.length > 0) {
            setIsRendering(true);
            const timer = setTimeout(() => {
                setRenderedQuests(quests);
                setIsRendering(false);
            }, 50);

            return () => clearTimeout(timer);
        }

        if (!isLoading && !isFetching && quests.length === 0) {
            setRenderedQuests([]);
            setIsRendering(false);
        }
    }, [quests, isLoading, isFetching]);

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
                            {renderedQuests.map((quest) => {
                                const questId = quest.id || quest.name || 'unknown-quest';
                                const objectives = quest.objectives || [];
                                const allObjectivesDone = objectives.length > 0 && objectives.every((obj) => obj.state === EQuestState.Completed);
                                const isQuestCompleted = quest.completed ?? false;

                                return (
                                    <Accordion.Item key={questId} value={quest.name || questId}>
                                        <Accordion.Control>
                                            <Group justify="space-between" wrap="nowrap" pr="xs">
                                                <Text fw={600} size="sm" truncate>
                                                    {quest.name || 'Unnamed Quest'}
                                                </Text>

                                                <Button
                                                    size="xs"
                                                    color="green"
                                                    variant="light"
                                                    leftSection={<IconCheck size={14} />}
                                                    disabled={!allObjectivesDone || isQuestCompleted}
                                                    loading={completeMutation.isPending && completeMutation.variables?.questId === questId && !completeMutation.variables?.objectiveId}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        completeMutation.mutate({ questId });
                                                    }}
                                                >
                                                    {isQuestCompleted ? 'Completed' : 'Finish Quest'}
                                                </Button>
                                            </Group>
                                        </Accordion.Control>

                                        <Accordion.Panel>
                                            <Stack gap="xs">
                                                <Text size="sm" c="dimmed" style={{ whiteSpace: 'pre-line' }}>
                                                    {quest.description || 'No description available.'}
                                                </Text>

                                                {objectives.length > 0 && (
                                                    <Paper withBorder p="xs" radius="sm" mt="xs" style={{ backgroundColor: 'var(--mantine-color-dark-6)' }}>
                                                        <Stack gap={8}>
                                                            <Text size="xs" fw={600}>
                                                                Objectives ({objectives.length}):
                                                            </Text>
                                                            {objectives.map((obj) => {
                                                                const objectiveId = obj.id || obj.description || 'unknown-obj';
                                                                const isObjComplete = obj.state === EQuestState.Completed;
                                                                const progress = obj.progress ?? 0;

                                                                return (
                                                                    <Group key={`${questId}-${objectiveId}`} justify="space-between" wrap="nowrap">
                                                                        <Text size="xs" style={{ flex: 1 }}>
                                                                            • {obj.description || 'No description'}{' '}
                                                                            {progress > 0 && (
                                                                                <Text span c="dimmed">
                                                                                    ({progress})
                                                                                </Text>
                                                                            )}
                                                                        </Text>

                                                                        {!isObjComplete ? (
                                                                            <Tooltip label="Complete Objective" openDelay={200}>
                                                                                <ActionIcon
                                                                                    size="xs"
                                                                                    color="green"
                                                                                    variant="filled"
                                                                                    loading={completeMutation.isPending && completeMutation.variables?.questId === questId && completeMutation.variables?.objectiveId === objectiveId}
                                                                                    onClick={() =>
                                                                                        completeMutation.mutate({
                                                                                            questId,
                                                                                            objectiveId,
                                                                                        })
                                                                                    }
                                                                                >
                                                                                    <IconCheck size={12} />
                                                                                </ActionIcon>
                                                                            </Tooltip>
                                                                        ) : (
                                                                            <Text size="xs" c="green" fw={600}>
                                                                                Done
                                                                            </Text>
                                                                        )}
                                                                    </Group>
                                                                );
                                                            })}
                                                        </Stack>
                                                    </Paper>
                                                )}
                                            </Stack>
                                        </Accordion.Panel>
                                    </Accordion.Item>
                                );
                            })}
                        </Accordion>
                    </ScrollArea.Autosize>
                )}
            </Box>
        </Modal>
    );
}
