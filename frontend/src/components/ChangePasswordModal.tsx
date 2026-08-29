import { Button, Modal, PasswordInput, Stack } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconLock } from '@tabler/icons-react';
import { useMutation } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';

interface ChangePasswordModalProps {
    opened: boolean;
    onClose: () => void;
}

export function ChangePasswordModal({ opened, onClose }: ChangePasswordModalProps) {
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const changePasswordMutation = useMutation({
        mutationFn: async () => {
            if (newPassword !== confirmPassword) {
                throw new Error('New passwords do not match');
            }
            return api.post('/account/change-password', {
                currentPassword,
                newPassword,
            });
        },
        onSuccess: () => {
            notifications.show({ color: 'green', message: 'Password changed successfully' });
            handleClose();
        },
        onError: (err: unknown) => {
            const message =
                err instanceof AxiosError
                    ? err.response?.data?.message || 'Failed to change password'
                    : err instanceof Error
                      ? err.message
                      : 'Failed to change password';
            notifications.show({ color: 'red', message });
        },
    });

    const handleClose = () => {
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
        onClose();
    };

    return (
        <Modal opened={opened} onClose={handleClose} title="Change Password" centered>
            <Stack gap="md">
                <PasswordInput
                    label="Current Password"
                    placeholder="Your current password"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.currentTarget.value)}
                />
                <PasswordInput
                    label="New Password"
                    placeholder="Your new password"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.currentTarget.value)}
                />
                <PasswordInput
                    label="Confirm New Password"
                    placeholder="Confirm new password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.currentTarget.value)}
                />
                <Button
                    fullWidth
                    mt="md"
                    leftSection={<IconLock size={16} />}
                    loading={changePasswordMutation.isPending}
                    onClick={() => changePasswordMutation.mutate()}
                >
                    Update Password
                </Button>
            </Stack>
        </Modal>
    );
}
