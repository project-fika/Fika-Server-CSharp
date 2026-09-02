import { Accordion, ActionIcon, Anchor, Autocomplete, Badge, Box, Button, Card, Group, Image, Loader, Modal, Paper, ScrollArea, SimpleGrid, Stack, Text, Title, Tooltip } from '@mantine/core';
import { useDebouncedCallback } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { IconGift, IconListCheck, IconSearch, IconStar, IconUserCheck, IconX } from '@tabler/icons-react';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import type { QuestData, QuestSearchResultDto } from '../types/quests';
import { Traders } from '../types/traders';

interface QuestSearchModalProps {
    opened: boolean;
    onClose: () => void;
}

const traderNameMap: Record<string, string> = {
    [Traders.PRAPOR]: 'Prapor',
    [Traders.THERAPIST]: 'Therapist',
    [Traders.FENCE]: 'Fence',
    [Traders.SKIER]: 'Skier',
    [Traders.PEACEKEEPER]: 'Peacekeeper',
    [Traders.MECHANIC]: 'Mechanic',
    [Traders.RAGMAN]: 'Ragman',
    [Traders.JAEGER]: 'Jaeger',
    [Traders.LIGHTHOUSEKEEPER]: 'Lightkeeper',
    [Traders.BTR]: 'BTR',
    [Traders.REF]: 'Ref',
};

const getIconUrl = (templateId?: string) => (templateId ? `https://assets.tarkov.dev/${templateId}-icon.webp` : '/images/missing_item.png');

const getWikiUrl = (templateId?: string) => (templateId ? `https://tarkov.dev/item/${templateId}` : undefined);

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
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to load quest details' : 'Failed to load quest details';
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

    const objectives = resolvedQuest?.objectives || [];
    const hasObjectives = objectives.length > 0;
    const totalExp = resolvedQuest?.experienceRewards?.reduce((sum, item) => sum + (item.amount || 0), 0) || 0;
    const hasRewards = totalExp > 0 || (resolvedQuest?.traderRewards && resolvedQuest.traderRewards.length > 0) || (resolvedQuest?.itemRewards && resolvedQuest.itemRewards.length > 0);

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
                                <Title order={4}>{resolvedQuest.name || 'Unnamed Quest'}</Title>
                                <Text size="sm" c="dimmed" mt="xs" style={{ whiteSpace: 'pre-line' }}>
                                    {resolvedQuest.description || 'No description available.'}
                                </Text>
                            </Box>

                            {hasObjectives && (
                                <Accordion variant="separated" radius="sm" mt="xs">
                                    <Accordion.Item value="objectives">
                                        <Accordion.Control icon={<IconListCheck size={18} />}>
                                            <Text size="sm" fw={600}>
                                                Objectives ({objectives.length})
                                            </Text>
                                        </Accordion.Control>
                                        <Accordion.Panel>
                                            <ScrollArea.Autosize mah={200} type="auto">
                                                <Stack gap="xs">
                                                    {objectives.map((obj) => (
                                                        <Paper key={`${resolvedQuest.name}-${obj.description || 'obj'}`} p="xs" withBorder style={{ backgroundColor: 'var(--mantine-color-dark-6)' }}>
                                                            <Text size="sm">• {obj.description || 'No description'}</Text>
                                                        </Paper>
                                                    ))}
                                                </Stack>
                                            </ScrollArea.Autosize>
                                        </Accordion.Panel>
                                    </Accordion.Item>
                                </Accordion>
                            )}

                            {hasRewards && (
                                <Accordion variant="separated" radius="sm" defaultValue="rewards">
                                    <Accordion.Item value="rewards">
                                        <Accordion.Control icon={<IconGift size={18} />}>
                                            <Text size="sm" fw={600}>
                                                Rewards
                                            </Text>
                                        </Accordion.Control>
                                        <Accordion.Panel>
                                            <Stack gap="sm">
                                                {/* EXP & Trader Standing Badges */}
                                                <Group gap="xs">
                                                    {totalExp > 0 && (
                                                        <Badge color="blue" variant="light" leftSection={<IconStar size={12} />}>
                                                            +{totalExp.toLocaleString()} EXP
                                                        </Badge>
                                                    )}

                                                    {resolvedQuest.traderRewards?.map((trader) => {
                                                        const name = traderNameMap[trader.traderId] || 'Trader';
                                                        const amt = trader.amount ?? 0;
                                                        const formattedAmt = amt > 0 ? `+${amt}` : `${amt}`;
                                                        return (
                                                            <Badge key={`${trader.traderId}-${amt}`} color="green" variant="light" leftSection={<IconUserCheck size={12} />}>
                                                                {name}: {formattedAmt}
                                                            </Badge>
                                                        );
                                                    })}
                                                </Group>

                                                {/* Items Grid */}
                                                {resolvedQuest.itemRewards && resolvedQuest.itemRewards.length > 0 && (
                                                    <ScrollArea.Autosize mah={250} type="auto">
                                                        <SimpleGrid cols={{ base: 3, sm: 4, md: 5 }} spacing="xs">
                                                            {resolvedQuest.itemRewards.map((item) => {
                                                                const iconUrl = getIconUrl(item.itemId);
                                                                const wikiUrl = getWikiUrl(item.itemId);
                                                                const formattedAmount = item.amount ? `x${item.amount.toLocaleString()}` : '';
                                                                const itemKey = `${item.itemId}-${item.amount ?? 1}`;

                                                                const content = (
                                                                    <Card
                                                                        key={itemKey}
                                                                        withBorder
                                                                        p={4}
                                                                        radius="sm"
                                                                        style={{
                                                                            position: 'relative',
                                                                            backgroundColor: 'var(--mantine-color-dark-8)',
                                                                            aspectRatio: '1',
                                                                        }}
                                                                    >
                                                                        <Image src={iconUrl} fallbackSrc="/images/missing_item.png" alt={item.itemId} fit="contain" h="100%" w="100%" />
                                                                        {formattedAmount && (
                                                                            <Text
                                                                                size="xs"
                                                                                fw={700}
                                                                                style={{
                                                                                    position: 'absolute',
                                                                                    bottom: 2,
                                                                                    right: 4,
                                                                                    color: '#fff',
                                                                                    textShadow: '0 1px 3px rgba(0,0,0,0.9), 0 0 2px rgba(0,0,0,1)',
                                                                                    pointerEvents: 'none',
                                                                                }}
                                                                            >
                                                                                {formattedAmount}
                                                                            </Text>
                                                                        )}
                                                                    </Card>
                                                                );

                                                                return wikiUrl ? (
                                                                    <Tooltip key={itemKey} label="View on tarkov.dev" openDelay={200}>
                                                                        <Anchor href={wikiUrl} target="_blank" rel="noopener noreferrer" style={{ textDecoration: 'none' }}>
                                                                            {content}
                                                                        </Anchor>
                                                                    </Tooltip>
                                                                ) : (
                                                                    content
                                                                );
                                                            })}
                                                        </SimpleGrid>
                                                    </ScrollArea.Autosize>
                                                )}
                                            </Stack>
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
