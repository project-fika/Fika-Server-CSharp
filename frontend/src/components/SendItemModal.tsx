import {
    ActionIcon,
    Anchor,
    Autocomplete,
    Box,
    Button,
    Checkbox,
    Group,
    Image,
    Loader,
    Modal,
    NumberInput,
    Stack,
    Text,
    Textarea,
    Tooltip,
} from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useDebouncedCallback } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { IconCheck, IconInfoCircle, IconX } from '@tabler/icons-react';
import { AxiosError } from 'axios';
import dayjs from 'dayjs';
import { useState } from 'react';
import { api } from '../api/axiosClient';

export interface SendItemModel {
    itemName: string;
    templateId: string;
    amount: number;
    message: string;
    expirationDays: number;
    foundInRaid: boolean;
    useDate: boolean;
    date: Date | null;
}

interface ItemSearchResultDto {
    templateId: string;
    name: string;
}

interface ResolvedItemDto {
    templateId: string;
    name: string;
    description: string;
    maxItems: number;
}

interface SendItemModalProps {
    opened: boolean;
    onClose: () => void;
    onConfirm: (model: SendItemModel) => void;
    loading?: boolean;
}

const getIconUrl = (templateId?: string) => (templateId ? `https://assets.tarkov.dev/${templateId}-icon.webp` : '/images/missing_item.png');

const getWikiUrl = (templateId?: string) => (templateId ? `https://tarkov.dev/item/${templateId}` : undefined);

export function SendItemModal({ opened, onClose, onConfirm, loading }: SendItemModalProps) {
    const [itemName, setItemName] = useState('');
    const [searchResults, setSearchResults] = useState<ItemSearchResultDto[]>([]);
    const [resolvedItem, setResolvedItem] = useState<ResolvedItemDto | null>(null);
    const [isSearching, setIsSearching] = useState(false);

    const [amount, setAmount] = useState<number>(1);
    const [message, setMessage] = useState('');
    const [expirationDays, setExpirationDays] = useState<number>(7);
    const [foundInRaid, setFoundInRaid] = useState(false);
    const [useDate, setUseDate] = useState(false);
    const [date, setDate] = useState<Date | null>(new Date());

    const fetchSearchResults = useDebouncedCallback(async (val: string) => {
        if (!val || val.trim().length <= 1) {
            setSearchResults([]);
            setIsSearching(false);
            return;
        }

        try {
            const res = await api.get<ItemSearchResultDto[]>(`/tools/items/search?query=${encodeURIComponent(val)}`);
            setSearchResults(res.data);
        } catch (err: unknown) {
            setSearchResults([]);
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to search for items' : 'Failed to search for items';
            notifications.show({
                color: 'red',
                message,
            });
        } finally {
            setIsSearching(false);
        }
    }, 500);

    const handleSearchChange = (val: string) => {
        setItemName(val);
        if (!val) {
            setResolvedItem(null);
            setSearchResults([]);
            setIsSearching(false);
            return;
        }
        setIsSearching(true);
        fetchSearchResults(val);
    };

    const handleSelectItem = async (selectedItemName: string) => {
        setItemName(selectedItemName);
        const match = searchResults.find((x) => x.name === selectedItemName);
        if (!match) return;

        try {
            const res = await api.get<ResolvedItemDto>(`/tools/items/resolve/${encodeURIComponent(match.templateId)}`);
            setResolvedItem(res.data);
        } catch (err: unknown) {
            setResolvedItem(null);
            const message =
                err instanceof AxiosError ? err.response?.data?.message || 'Failed to resolve item details' : 'Failed to resolve item details';
            notifications.show({
                color: 'red',
                message,
            });
        }
    };

    const handleClearItem = () => {
        setItemName('');
        setResolvedItem(null);
        setSearchResults([]);
        setIsSearching(false);
    };

    const maxItems = resolvedItem?.maxItems || 10;
    const description = resolvedItem?.description || 'Select an item';
    const imgUrl = getIconUrl(resolvedItem?.templateId);
    const wikiUrl = getWikiUrl(resolvedItem?.templateId);

    const isFormValid = itemName.trim().length > 0 && amount >= 1 && expirationDays >= 1 && (!useDate || date !== null);

    const handleSubmit = () => {
        onConfirm({
            itemName,
            templateId: resolvedItem?.templateId || '',
            amount,
            message,
            expirationDays,
            foundInRaid,
            useDate,
            date,
        });
    };

    return (
        <Modal opened={opened} onClose={onClose} title="Select an item and an amount to send" size="lg" centered c="#c7c5b3" radius="lg">
            <Stack gap="sm">
                <Group align="flex-end" wrap="nowrap" gap="xs">
                    <Autocomplete
                        label="Item"
                        placeholder="Select an item..."
                        data={searchResults.map((x) => x.name)}
                        value={itemName}
                        onChange={handleSearchChange}
                        onOptionSubmit={handleSelectItem}
                        renderOption={({ option }) => {
                            const match = searchResults.find((x) => x.name === option.value);
                            const iconUrl = getIconUrl(match?.templateId);

                            return (
                                <Group gap="sm" wrap="nowrap">
                                    <Image src={iconUrl} fallbackSrc="/images/missing_item.png" w={32} h={32} fit="contain" />
                                    <Text size="sm" truncate>
                                        {option.value}
                                    </Text>
                                </Group>
                            );
                        }}
                        required
                        style={{ flex: 1, minWidth: 0 }}
                        rightSectionWidth={36}
                        rightSection={
                            <Box style={{ width: 16, height: 16, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                                {isSearching ? (
                                    <Loader size={16} />
                                ) : itemName ? (
                                    <ActionIcon variant="subtle" color="gray" size="sm" onClick={handleClearItem}>
                                        <IconX size={14} />
                                    </ActionIcon>
                                ) : null}
                            </Box>
                        }
                    />

                    <Tooltip
                        label={
                            <Group gap="xs">
                                <IconInfoCircle size={14} />
                                <Text size="xs" style={{ maxWidth: 300, wordWrap: 'break-word' }}>
                                    {description}
                                </Text>
                            </Group>
                        }
                        position="top"
                        withArrow
                    >
                        <Anchor href={wikiUrl} target="_blank" underline="never" style={{ flexShrink: 0 }}>
                            <Box style={{ position: 'relative', width: 64, height: 64, border: '2px solid #2d2d2f' }}>
                                <Image src={imgUrl} fallbackSrc="/images/missing_item.png" w={60} h={60} />
                                {itemName && <IconInfoCircle size={14} style={{ position: 'absolute', bottom: 2, right: 2, color: 'white' }} />}
                            </Box>
                        </Anchor>
                    </Tooltip>
                </Group>

                <NumberInput
                    label="Amount of items"
                    description={`How many items to send (max ${maxItems.toLocaleString()})`}
                    min={1}
                    max={maxItems}
                    value={amount}
                    onChange={(val) => setAmount(Number(val) || 1)}
                    required
                />

                <Textarea
                    label="Message"
                    placeholder="Message attached to the delivery"
                    rows={3}
                    value={message}
                    onChange={(e) => setMessage(e.currentTarget.value)}
                />

                <NumberInput
                    label="Days until expiration"
                    description="How many days until the message expires"
                    min={1}
                    max={31}
                    value={expirationDays}
                    onChange={(val) => setExpirationDays(Number(val) || 1)}
                    required
                />

                <Group mt="xs">
                    <Checkbox label="Found In Raid" checked={foundInRaid} onChange={(e) => setFoundInRaid(e.currentTarget.checked)} />
                    <Checkbox label="Specific Date" checked={useDate} onChange={(e) => setUseDate(e.currentTarget.checked)} />
                </Group>

                {useDate && (
                    <DateTimePicker
                        c="#c7c5b3"
                        label="Send Date & Time"
                        placeholder="Select Date & Time"
                        value={date}
                        onChange={(val) => setDate(val ? new Date(val) : null)}
                        minDate={new Date(Date.now() - 24 * 60 * 60 * 1000)}
                        popoverProps={{ withinPortal: true }}
                        clearable
                        presets={[
                            { value: dayjs().format('YYYY-MM-DD HH:mm:ss'), label: 'Today' },
                            { value: dayjs().add(1, 'day').format('YYYY-MM-DD HH:mm:ss'), label: 'Tomorrow' },
                            { value: dayjs().add(1, 'week').format('YYYY-MM-DD HH:mm:ss'), label: 'Next week' },
                            { value: dayjs().add(1, 'month').format('YYYY-MM-DD HH:mm:ss'), label: 'Next month' },
                        ]}
                        timePickerProps={{
                            withDropdown: true,
                            popoverProps: { withinPortal: false },
                        }}
                        valueFormat="DD MMM YYYY - HH:mm"
                        dropdownType="modal"
                        withNativeLevelSelect
                    />
                )}

                <Group justify="flex-end" mt="md">
                    <Button leftSection={<IconCheck size={16} />} disabled={!isFormValid} loading={loading} onClick={handleSubmit}>
                        Confirm
                    </Button>
                    <Button variant="default" leftSection={<IconX size={16} />} onClick={onClose}>
                        Cancel
                    </Button>
                </Group>
            </Stack>
        </Modal>
    );
}
