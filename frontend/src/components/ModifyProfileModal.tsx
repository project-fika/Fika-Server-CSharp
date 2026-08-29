import { Button, Divider, Group, Modal, NumberInput, Stack, Text } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconCheck, IconCircleMinus, IconCirclePlus, IconMailForward } from '@tabler/icons-react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import type { ProfileResponse } from '../pages/ProfilesPage';
import { SendItemModal, type SendItemModel } from './SendItemModal';

interface ModifyProfileModalProps {
    profile: ProfileResponse | null;
    onClose: () => void;
}

export function ModifyProfileModal({ profile, onClose }: ModifyProfileModalProps) {
    const queryClient = useQueryClient();
    const [hasFleaBan, setHasFleaBan] = useState<boolean>(profile?.hasFleaBan ?? false);

    const [showBanConfirm, setShowBanConfirm] = useState(false);
    const [amountOfDays, setAmountOfDays] = useState<number>(7);
    const [showUnbanConfirm, setShowUnbanConfirm] = useState(false);
    const [showSendItemModal, setShowSendItemModal] = useState(false);

    const addBanMutation = useMutation({
        mutationFn: (days: number) =>
            api.post('/profiles/add-flea-ban', {
                profileId: profile?.profileId,
                amountOfDays: Math.min(Math.max(days, 0), 9999),
            }),
        onSuccess: () => {
            setHasFleaBan(true);
            setShowBanConfirm(false);
            notifications.show({ color: 'green', message: 'Flea ban added successfully' });
            queryClient.invalidateQueries({ queryKey: ['profiles'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to add flea ban' : 'Failed to add flea ban';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    const removeBanMutation = useMutation({
        mutationFn: () =>
            api.post('/profiles/remove-flea-ban', {
                profileId: profile?.profileId,
            }),
        onSuccess: () => {
            setHasFleaBan(false);
            setShowUnbanConfirm(false);
            notifications.show({ color: 'green', message: 'Flea ban removed successfully' });
            queryClient.invalidateQueries({ queryKey: ['profiles'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to remove flea ban' : 'Failed to remove flea ban';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    const sendItemMutation = useMutation({
        mutationFn: async (model: SendItemModel) => {
            if (!profile) return;

            const requestPayload = {
                profileId: profile.profileId,
                itemTemplate: model.templateId,
                itemTpl: model.templateId,
                amount: model.amount,
                message: model.message,
                foundInRaid: model.foundInRaid,
                fir: model.foundInRaid,
                expirationDays: model.expirationDays,
            };

            if (model.useDate && model.date) {
                return api.post('/tools/schedule/single', {
                    request: requestPayload,
                    sendDate: model.date.toISOString(),
                });
            }

            return api.post('/profiles/send-item', requestPayload);
        },
        onSuccess: (_, variables) => {
            setShowSendItemModal(false);
            if (variables.useDate && variables.date) {
                notifications.show({
                    color: 'green',
                    message: `The item was queued to be sent to ${profile?.nickname} at ${variables.date.toLocaleString()}.`,
                });
            } else {
                notifications.show({
                    color: 'green',
                    message: `[${variables.itemName}] was successfully sent to ${profile?.nickname}`,
                });
            }
        },
        onError: (err: unknown) => {
            const message =
                err instanceof AxiosError
                    ? err.response?.data?.message || err.message
                    : err instanceof Error
                      ? err.message
                      : 'Error sending item to user';
            notifications.show({
                color: 'red',
                message,
            });
        },
    });

    if (!profile) return null;

    return (
        <>
            <Modal opened={!!profile} onClose={onClose} title={`Modify ${profile.nickname}`} centered>
                <Stack gap="md">
                    <Group grow gap="xs">
                        <Button leftSection={<IconCirclePlus size={16} />} disabled={hasFleaBan} onClick={() => setShowBanConfirm(true)}>
                            Add Flea Ban
                        </Button>

                        <Button leftSection={<IconCircleMinus size={16} />} disabled={!hasFleaBan} onClick={() => setShowUnbanConfirm(true)}>
                            Remove Flea Ban
                        </Button>
                    </Group>

                    <Divider />

                    <Button fullWidth leftSection={<IconMailForward size={16} />} onClick={() => setShowSendItemModal(true)}>
                        Send Item
                    </Button>

                    <Divider />

                    <Group justify="center" gap="sm">
                        <Button variant="default" leftSection={<IconCheck size={16} />} onClick={onClose}>
                            Ok
                        </Button>
                        {/* <Button variant="default" leftSection={<IconX size={16} />} onClick={onClose}>
                            Cancel
                        </Button> */}
                    </Group>
                </Stack>
            </Modal>

            {/* Confirm Add Flea Ban Modal */}
            <Modal opened={showBanConfirm} onClose={() => setShowBanConfirm(false)} title="Confirm Flea Ban" centered>
                <Stack gap="sm">
                    <NumberInput
                        label="Amount of Days"
                        description="0 means 9999 days"
                        min={0}
                        max={9999}
                        value={amountOfDays}
                        onChange={(val) => setAmountOfDays(Number(val) || 0)}
                    />
                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={() => setShowBanConfirm(false)}>
                            Cancel
                        </Button>
                        <Button color="red" loading={addBanMutation.isPending} onClick={() => addBanMutation.mutate(amountOfDays)}>
                            Confirm Ban
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            {/* Confirm Remove Flea Ban Modal */}
            <Modal opened={showUnbanConfirm} onClose={() => setShowUnbanConfirm(false)} title="Confirmation" centered>
                <Stack gap="sm">
                    <Text size="sm">Are you sure you want to remove the Flea Ban?</Text>
                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={() => setShowUnbanConfirm(false)}>
                            No
                        </Button>
                        <Button loading={removeBanMutation.isPending} onClick={() => removeBanMutation.mutate()}>
                            Yes
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            {/* Send Item Modal for Individual Player */}
            <SendItemModal
                opened={showSendItemModal}
                onClose={() => setShowSendItemModal(false)}
                loading={sendItemMutation.isPending}
                onConfirm={(model) => sendItemMutation.mutate(model)}
            />
        </>
    );
}
