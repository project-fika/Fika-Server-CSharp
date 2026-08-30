import { Accordion, ActionIcon, Autocomplete, Box, Button, Group, Loader, Modal, Paper, ScrollArea, Stack, Text, Title } from '@mantine/core';
import { useDebouncedCallback } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { IconListCheck, IconSearch, IconX } from '@tabler/icons-react';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import type { QuestData, QuestSearchResultDto } from '../types/quests';

interface QuestSearchModalProps {
    opened: boolean;
    onClose: () => void;
}

export function QuestSearchModal({ opened, onClose }: QuestSearchModalProps) {
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState<QuestSearchResultDto[]>([]);
    const [resolvedQuest, setResolvedQuest] = useState<QuestData | null>(null);
    const [isSearching, setIsSearching] = useState(false);
    const [isLoadingQuest, setIsLoadingQuest] = useState(false);

    const fetchSearchResults = useDebouncedCallback(async (val: string) => {
        if (!val || val.trim().length <= 1) {
            setSearchResults([]);
            setIsSearching(false);
            return;
        }

        try {
            const res = await api.get<QuestSearchResultDto[]>(`/tools/quests/search?query=${encodeURIComponent(val)}`);
            setSearchResults(res.data);
        } catch (err: unknown) {
            setSearchResults([]);
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to search quests' : 'Failed to search quests';
            notifications.show({
                color: 'red',
                message,
            });
        } finally {
            setIsSearching(false);
        }
    }, 500);

    const handleSearchChange = (val: string) => {
        setSearchQuery(val);
        if (!val) {
            setResolvedQuest(null);
            setSearchResults([]);
            setIsSearching(false);
            return;
        }
        setIsSearching(true);
        fetchSearchResults(val);
    };

    const handleSelectQuest = async (selectedQuestName: string) => {
        setSearchQuery(selectedQuestName);
        const match = searchResults.find((x) => x.name === selectedQuestName);

        if (!match?.templateId) {
            notifications.show({
                color: 'red',
                message: 'Failed to resolve quest ID from selection.',
            });
            return;
        }

        setIsLoadingQuest(true);
        try {
            const res = await api.get<QuestData>(`/tools/quests/resolve/${encodeURIComponent(match.templateId)}`);
            setResolvedQuest(res.data);
        } catch (err: unknown) {
            setResolvedQuest(null);
            const message =
                err instanceof AxiosError ? err.response?.data?.message || 'Failed to load quest details' : 'Failed to load quest details';
            notifications.show({
                color: 'red',
                message,
            });
        } finally {
            setIsLoadingQuest(false);
        }
    };

    const handleClear = () => {
        setSearchQuery('');
        setResolvedQuest(null);
        setSearchResults([]);
        setIsSearching(false);
    };

    const handleCloseModal = () => {
        handleClear();
        onClose();
    };

    const hasObjectives = resolvedQuest?.objectives && resolvedQuest.objectives.length > 0;

    return (
        <Modal opened={opened} onClose={handleCloseModal} title="Search Quests" size="lg" centered radius="md">
            <Stack gap="md">
                <Autocomplete
                    label="Quest Name"
                    placeholder="Search for a quest..."
                    leftSection={<IconSearch size={16} />}
                    data={searchResults.map((x) => x.name)}
                    value={searchQuery}
                    onChange={handleSearchChange}
                    onOptionSubmit={handleSelectQuest}
                    required
                    rightSectionWidth={36}
                    rightSection={
                        <Box style={{ width: 16, height: 16, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            {isSearching ? (
                                <Loader size={16} />
                            ) : searchQuery ? (
                                <ActionIcon variant="subtle" color="gray" size="sm" onClick={handleClear}>
                                    <IconX size={14} />
                                </ActionIcon>
                            ) : null}
                        </Box>
                    }
                />

                {isLoadingQuest && (
                    <Stack align="center" py="xl">
                        <Loader size="md" />
                        <Text size="sm" c="dimmed">
                            Loading quest details...
                        </Text>
                    </Stack>
                )}

                {!isLoadingQuest && resolvedQuest && (
                    <Paper withBorder p="md" radius="sm" style={{ backgroundColor: 'var(--mantine-color-dark-7)' }}>
                        <Stack gap="sm">
                            <Box>
                                <Title order={4}>{resolvedQuest.name}</Title>
                                <Text size="sm" c="dimmed" mt="xs" style={{ whiteSpace: 'pre-line' }}>
                                    {resolvedQuest.description || 'No description available.'}
                                </Text>
                            </Box>

                            {hasObjectives && (
                                <Accordion variant="separated" radius="sm" mt="xs">
                                    <Accordion.Item value="objectives">
                                        <Accordion.Control icon={<IconListCheck size={18} />}>
                                            <Text size="sm" fw={600}>
                                                Objectives ({resolvedQuest.objectives.length})
                                            </Text>
                                        </Accordion.Control>
                                        <Accordion.Panel>
                                            <ScrollArea.Autosize mah={250} type="auto">
                                                <Stack gap="xs">
                                                    {resolvedQuest.objectives.map((obj) => (
                                                        <Paper
                                                            key={`${resolvedQuest.name}-${obj.description}`}
                                                            p="xs"
                                                            withBorder
                                                            style={{ backgroundColor: 'var(--mantine-color-dark-6)' }}
                                                        >
                                                            <Text size="sm">• {obj.description}</Text>
                                                        </Paper>
                                                    ))}
                                                </Stack>
                                            </ScrollArea.Autosize>
                                        </Accordion.Panel>
                                    </Accordion.Item>
                                </Accordion>
                            )}
                        </Stack>
                    </Paper>
                )}

                <Group justify="flex-end" mt="xs">
                    <Button variant="default" onClick={handleCloseModal}>
                        Close
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
