import { Button, Paper, PasswordInput, Stack, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconLock } from '@tabler/icons-react';
import { useMutation } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';

export function ChangePasswordPage() {
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
            setCurrentPassword('');
            setNewPassword('');
            setConfirmPassword('');
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

    return (
        <Stack maw={400} mx="auto" pt="xl">
            <Title order={2} mb="sm">
                Change Password
            </Title>
            <Paper withBorder p="xl" radius="md">
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
            </Paper>
        </Stack>
    );
}
