import {
    ActionIcon,
    Badge,
    Button,
    Card,
    Checkbox,
    Group,
    LoadingOverlay,
    Modal,
    MultiSelect,
    Paper,
    PasswordInput,
    Stack,
    Table,
    Text,
    TextInput,
    Title,
    Tooltip,
} from '@mantine/core';
import { useDisclosure, useMediaQuery } from '@mantine/hooks';
import { notifications } from '@mantine/notifications';
import { IconLock, IconPlus, IconSettings, IconTrash } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import type { CreateUserPayload, UserDto } from '../types/accounts';

const AVAILABLE_ROLES = ['Admin', 'Moderator'];

export function AccountsPage() {
    const { user: currentUser } = useAuth();
    const queryClient = useQueryClient();
    const isMobile = useMediaQuery('(max-width: 768px)');

    // Modal States
    const [addOpened, { open: openAdd, close: closeAdd }] = useDisclosure(false);
    const [editUser, setEditUser] = useState<UserDto | null>(null);
    const [deleteTarget, setDeleteTarget] = useState<UserDto | null>(null);

    // Form States
    const [newUsername, setNewUsername] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [newRoles, setNewRoles] = useState<string[]>([]);
    const [editRoles, setEditRoles] = useState<string[]>([]);

    // Fetch Users
    const { data: users = [], isLoading } = useQuery<UserDto[]>({
        queryKey: ['users'],
        queryFn: async () => (await api.get<UserDto[]>('/accounts')).data,
    });

    const resetAddForm = () => {
        setNewUsername('');
        setNewPassword('');
        setNewRoles([]);
    };

    // Mutations
    const createMutation = useMutation({
        mutationFn: (data: CreateUserPayload) => api.post('/accounts', data),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data.message });
            queryClient.invalidateQueries({ queryKey: ['users'] });
            closeAdd();
            resetAddForm();
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Error creating user' : 'Error creating user';
            notifications.show({ color: 'red', message });
        },
    });

    const rolesMutation = useMutation({
        mutationFn: ({ id, roles }: { id: string; roles: string[] }) => api.put(`/accounts/${id}/roles`, { roles }),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data.message });
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setEditUser(null);
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Error updating roles' : 'Error updating roles';
            notifications.show({ color: 'red', message });
        },
    });

    const lockMutation = useMutation({
        mutationFn: (id: string) => api.post(`/accounts/${id}/toggle-lock`),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data.message });
            queryClient.invalidateQueries({ queryKey: ['users'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Error toggling lock' : 'Error toggling lock';
            notifications.show({ color: 'red', message });
        },
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) => api.delete(`/accounts/${id}`),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data.message });
            queryClient.invalidateQueries({ queryKey: ['users'] });
            setDeleteTarget(null);
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Error deleting user' : 'Error deleting user';
            notifications.show({ color: 'red', message });
        },
    });

    const validateAction = (targetUser: UserDto): boolean => {
        if (targetUser.userName === currentUser?.username) {
            notifications.show({ color: 'yellow', message: 'You cannot modify your own account!' });
            return false;
        }
        if (targetUser.userName === 'admin') {
            notifications.show({ color: 'yellow', message: 'You cannot modify the root user' });
            return false;
        }
        return true;
    };

    return (
        <Stack gap="md" style={{ width: '100%', maxWidth: '100%' }}>
            <Group justify="space-between" align="center" wrap="wrap">
                <Title order={2}>Account Management</Title>
                <Button leftSection={<IconPlus size={18} />} onClick={openAdd}>
                    Add New Account
                </Button>
            </Group>

            {isMobile ? (
                /* Mobile Layout: Responsive Card List */
                <Stack gap="sm">
                    {users.map((row) => {
                        const isLocked = Boolean(row.lockoutEnd && new Date(row.lockoutEnd) > new Date());

                        return (
                            <Card key={row.id} withBorder p="sm" radius="md">
                                <Stack gap="xs">
                                    <Group justify="space-between" align="center">
                                        <Text fw={700} size="md">
                                            {row.userName}
                                        </Text>
                                        <Group gap="xs">
                                            <Text size="xs" c="dimmed">
                                                Locked:
                                            </Text>
                                            <Checkbox checked={isLocked} readOnly />
                                        </Group>
                                    </Group>

                                    <Group gap="xs" wrap="wrap">
                                        {row.roles.map((role) => (
                                            <Badge key={role} size="sm">
                                                {role}
                                            </Badge>
                                        ))}
                                    </Group>

                                    <Group justify="flex-end" gap="xs" mt="xs">
                                        <ActionIcon color="red" variant="filled" onClick={() => validateAction(row) && setDeleteTarget(row)}>
                                            <IconTrash size={16} />
                                        </ActionIcon>
                                        <ActionIcon variant="filled" onClick={() => validateAction(row) && lockMutation.mutate(row.id)}>
                                            <IconLock size={16} />
                                        </ActionIcon>
                                        <ActionIcon
                                            variant="filled"
                                            onClick={() => {
                                                if (validateAction(row)) {
                                                    setEditUser(row);
                                                    setEditRoles(row.roles);
                                                }
                                            }}
                                        >
                                            <IconSettings size={16} />
                                        </ActionIcon>
                                    </Group>
                                </Stack>
                            </Card>
                        );
                    })}
                </Stack>
            ) : (
                /* Desktop Layout: Standard Table */
                <Paper withBorder p="md" pos="relative" style={{ overflow: 'hidden' }}>
                    <LoadingOverlay visible={isLoading} />
                    <Table verticalSpacing="sm" highlightOnHover>
                        <Table.Thead>
                            <Table.Tr>
                                <Table.Th>Username</Table.Th>
                                <Table.Th>Roles</Table.Th>
                                <Table.Th style={{ textAlign: 'center' }}>Locked</Table.Th>
                                <Table.Th style={{ textAlign: 'right' }}>Actions</Table.Th>
                            </Table.Tr>
                        </Table.Thead>
                        <Table.Tbody>
                            {users.map((row) => {
                                const isLocked = Boolean(row.lockoutEnd && new Date(row.lockoutEnd) > new Date());
                                const lockText = isLocked && row.lockoutEnd ? `Expires at ${new Date(row.lockoutEnd).toLocaleString()}` : 'Active';

                                return (
                                    <Table.Tr key={row.id}>
                                        <Table.Td style={{ whiteSpace: 'nowrap' }}>{row.userName}</Table.Td>
                                        <Table.Td>
                                            <Group gap="xs" wrap="wrap">
                                                {row.roles.map((role) => (
                                                    <Badge key={role} size="sm">
                                                        {role}
                                                    </Badge>
                                                ))}
                                            </Group>
                                        </Table.Td>
                                        <Table.Td style={{ textAlign: 'center' }}>
                                            <Tooltip label={lockText}>
                                                <Checkbox checked={isLocked} readOnly style={{ display: 'inline-block' }} />
                                            </Tooltip>
                                        </Table.Td>
                                        <Table.Td style={{ textAlign: 'right' }}>
                                            <Group gap="xs" justify="flex-end" wrap="nowrap">
                                                <Tooltip label={`Delete ${row.userName}`}>
                                                    <ActionIcon
                                                        color="red"
                                                        variant="filled"
                                                        onClick={() => validateAction(row) && setDeleteTarget(row)}
                                                    >
                                                        <IconTrash size={16} />
                                                    </ActionIcon>
                                                </Tooltip>

                                                <Tooltip label={`Toggle lock ${row.userName}`}>
                                                    <ActionIcon variant="filled" onClick={() => validateAction(row) && lockMutation.mutate(row.id)}>
                                                        <IconLock size={16} />
                                                    </ActionIcon>
                                                </Tooltip>

                                                <Tooltip label={`Edit ${row.userName}`}>
                                                    <ActionIcon
                                                        variant="filled"
                                                        onClick={() => {
                                                            if (validateAction(row)) {
                                                                setEditUser(row);
                                                                setEditRoles(row.roles);
                                                            }
                                                        }}
                                                    >
                                                        <IconSettings size={16} />
                                                    </ActionIcon>
                                                </Tooltip>
                                            </Group>
                                        </Table.Td>
                                    </Table.Tr>
                                );
                            })}
                        </Table.Tbody>
                    </Table>
                </Paper>
            )}

            {/* Add User Modal */}
            <Modal opened={addOpened} onClose={closeAdd} title="Add Account">
                <Stack gap="sm">
                    <TextInput label="Username" required value={newUsername} onChange={(e) => setNewUsername(e.target.value)} />
                    <PasswordInput label="Password" required value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
                    <MultiSelect label="Roles" data={AVAILABLE_ROLES} value={newRoles} onChange={setNewRoles} />
                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={closeAdd}>
                            Cancel
                        </Button>
                        <Button
                            loading={createMutation.isPending}
                            onClick={() =>
                                createMutation.mutate({
                                    username: newUsername,
                                    password: newPassword,
                                    roles: newRoles,
                                })
                            }
                        >
                            Create
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            {/* Edit User Roles Modal */}
            <Modal opened={!!editUser} onClose={() => setEditUser(null)} title={`Modify Account (${editUser?.userName})`}>
                <Stack gap="sm">
                    <MultiSelect label="Roles" data={AVAILABLE_ROLES} value={editRoles} onChange={setEditRoles} />
                    <Group justify="flex-end" mt="md">
                        <Button variant="default" onClick={() => setEditUser(null)}>
                            Cancel
                        </Button>
                        <Button
                            loading={rolesMutation.isPending}
                            onClick={() => editUser && rolesMutation.mutate({ id: editUser.id, roles: editRoles })}
                        >
                            Save
                        </Button>
                    </Group>
                </Stack>
            </Modal>

            {/* Delete Confirmation Modal */}
            <Modal opened={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Confirmation">
                <Text size="sm">
                    Are you sure you want to delete user <b>{deleteTarget?.userName}</b>?
                </Text>
                <Group justify="flex-end" mt="md">
                    <Button variant="default" onClick={() => setDeleteTarget(null)}>
                        No
                    </Button>
                    <Button color="red" loading={deleteMutation.isPending} onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}>
                        Yes
                    </Button>
                </Group>
            </Modal>
        </Stack>
    );
}
