import { Accordion, ActionIcon, Box, Button, Checkbox, Group, LoadingOverlay, Modal, Paper, ScrollArea, Stack, Text, Tooltip } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconCheck } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useEffect, useMemo, useState } from 'react';
import { api } from '../api/axiosClient';
import type { ProfileResponse } from '../types/profiles';
import { EQuestState, type QuestData, type QuestObjective } from '../types/quests';

interface ProfileDetailedQuestsModalProps {
    profile: ProfileResponse | null;
    opened: boolean;
    onClose: () => void;
}

interface CompleteQuestPayload {
    questId: string;
    objectiveId?: string;
}

export function ProfileDetailedQuestsModal({ profile, opened, onClose }: ProfileDetailedQuestsModalProps) {
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
        queryKey: ['profileDetailedQuests', profile?.profileId],
        queryFn: async () => {
            if (!profile?.profileId) return [];
            const res = await api.get<QuestData[]>(`/profiles/quests?profileId=${encodeURIComponent(profile.profileId)}`);
            return res.data;
        },
        enabled: !!profile?.profileId && opened,
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
            queryClient.invalidateQueries({ queryKey: ['profileDetailedQuests', profile?.profileId] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to complete quest action' : 'Failed to complete quest action';
            notifications.show({ color: 'red', message });
        },
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

    const renderQuestAccordionList = (questList: QuestData[]) => {
        if (questList.length === 0) {
            return (
                <Text size="xs" c="dimmed" py="xs">
                    None found.
                </Text>
            );
        }

        return (
            <Accordion variant="separated" radius="sm">
                {questList.map((quest) => {
                    const questKey = quest.id || quest.name || 'quest';
                    const objectives = quest.objectives || [];
                    const allObjectivesDone = objectives.length > 0 && objectives.every((obj) => obj.state === EQuestState.Completed);
                    const isQuestCompleted = quest.completed ?? false;

                    return (
                        <Accordion.Item key={questKey} value={quest.name || questKey} style={{ border: '1px solid var(--mantine-color-dark-3)' }}>
                            <Accordion.Control>
                                <Group justify="space-between" wrap="nowrap" pr="xs">
                                    <Text fw={600} size="sm" truncate style={{ flex: 1 }}>
                                        {quest.name || 'Unnamed Quest'}
                                    </Text>

                                    <Button
                                        size="xs"
                                        leftSection={<IconCheck size={14} />}
                                        disabled={!allObjectivesDone || isQuestCompleted}
                                        loading={completeMutation.isPending && completeMutation.variables?.questId === questKey && !completeMutation.variables?.objectiveId}
                                        style={{
                                            border: '1px solid var(--mantine-color-dark-3)',
                                        }}
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            completeMutation.mutate({ questId: questKey });
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
                                            <Accordion variant="subtle" radius="xs">
                                                <Accordion.Item value="objectives-list">
                                                    <Accordion.Control p={0}>
                                                        <Text size="xs" fw={600}>
                                                            Objectives ({objectives.length})
                                                        </Text>
                                                    </Accordion.Control>
                                                    <Accordion.Panel pt="xs">
                                                        <Stack gap="xs">
                                                            {objectives.map((obj: QuestObjective) => {
                                                                const objKey = obj.id || obj.description || 'obj';
                                                                const progress = obj.progress ?? 0;
                                                                const target = obj.target ?? 0;
                                                                const isObjComplete = obj.state === EQuestState.Completed;

                                                                return (
                                                                    <Group key={`${questKey}-${objKey}`} justify="space-between" align="center" wrap="nowrap">
                                                                        <Group gap="xs" align="flex-start" wrap="nowrap" style={{ flex: 1 }}>
                                                                            <Checkbox checked={isObjComplete} readOnly size="xs" mt={2} />
                                                                            <Text size="xs">
                                                                                {obj.description || 'No description'}{' '}
                                                                                {progress > 0 && (
                                                                                    <Text span c="dimmed">
                                                                                        ({progress}/{target})
                                                                                    </Text>
                                                                                )}
                                                                            </Text>
                                                                        </Group>

                                                                        {!isObjComplete ? (
                                                                            <Tooltip label="Complete Objective" openDelay={200}>
                                                                                <ActionIcon
                                                                                    size="xs"
                                                                                    color="green"
                                                                                    variant="filled"
                                                                                    loading={completeMutation.isPending && completeMutation.variables?.questId === questKey && completeMutation.variables?.objectiveId === objKey}
                                                                                    onClick={() =>
                                                                                        completeMutation.mutate({
                                                                                            questId: questKey,
                                                                                            objectiveId: objKey,
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
                                                    </Accordion.Panel>
                                                </Accordion.Item>
                                            </Accordion>
                                        </Paper>
                                    )}
                                </Stack>
                            </Accordion.Panel>
                        </Accordion.Item>
                    );
                })}
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
                        <Accordion multiple defaultValue={[]} variant="filled" radius="md" style={{ border: '1px solid var(--mantine-color-dark-3)' }}>
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
