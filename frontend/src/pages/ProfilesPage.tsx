import { CodeHighlight } from '@mantine/code-highlight';
import {
    Box,
    Button,
    Card,
    Checkbox,
    FileButton,
    Group,
    Loader,
    Modal,
    Paper,
    ScrollArea,
    Stack,
    Table,
    Text,
    TextInput,
    Title,
    Tooltip,
} from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconEdit, IconEye, IconSearch, IconUpload } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useMemo, useState } from 'react';
import { api } from '../api/axiosClient';
import { ModifyProfileModal } from '../components/ModifyProfileModal';

export interface ProfileResponse {
    nickname: string;
    profileId: string;
    level: number;
    hasFleaBan: boolean;
}

export function ProfilesPage() {
    const queryClient = useQueryClient();
    const [searchString, setSearchString] = useState('');

    const [selectedModifyProfile, setSelectedModifyProfile] = useState<ProfileResponse | null>(null);
    const [viewJsonData, setViewJsonData] = useState<{ title: string; json: string } | null>(null);
    const [pendingUploadFile, setPendingUploadFile] = useState<File | null>(null);

    const { data: profiles = [], isLoading } = useQuery<ProfileResponse[]>({
        queryKey: ['profiles'],
        queryFn: async () => (await api.get<ProfileResponse[]>('/profiles')).data,
    });

    const filteredProfiles = useMemo(() => {
        if (!searchString.trim()) return profiles;
        const query = searchString.toLowerCase();
        return profiles.filter((p) => p.nickname.toLowerCase().includes(query) || p.profileId.toLowerCase().includes(query));
    }, [profiles, searchString]);

    const uploadMutation = useMutation({
        mutationFn: (jsonContent: string) => api.post('/profiles/upload', jsonContent),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data || 'Profile uploaded successfully' });
            queryClient.invalidateQueries({ queryKey: ['profiles'] });
            setPendingUploadFile(null);
        },
        onError: (err: unknown) => {
            const message =
                err instanceof AxiosError
                    ? err.response?.data?.message || 'There was an error uploading the profile'
                    : 'There was an error uploading the profile';
            notifications.show({
                color: 'red',
                message,
            });
            setPendingUploadFile(null);
        },
    });

    const handleViewProfile = async (profile: ProfileResponse) => {
        try {
            const res = await api.get<string>(`/profiles/raw?profileId=${encodeURIComponent(profile.profileId)}`);

            let formattedJson = '';
            if (typeof res.data === 'string') {
                try {
                    formattedJson = JSON.stringify(JSON.parse(res.data), null, 2);
                } catch {
                    formattedJson = res.data;
                }
            } else {
                formattedJson = JSON.stringify(res.data, null, 2);
            }

            setViewJsonData({
                title: `Viewing ${profile.nickname}`,
                json: formattedJson,
            });
        } catch (err: unknown) {
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to load profile data' : 'Failed to load profile data';
            notifications.show({
                color: 'red',
                message,
            });
        }
    };

    const handleFileConfirmUpload = () => {
        if (!pendingUploadFile) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            const content = e.target?.result as string;
            if (content) {
                uploadMutation.mutate(content);
            }
        };
        reader.readAsText(pendingUploadFile);
    };

    if (isLoading) {
        return (
            <Stack align="center" justify="center" h={300}>
                <Loader size="lg" />
                <Text c="dimmed">Waiting for server...</Text>
            </Stack>
        );
    }

    return (
        <Stack gap="md" style={{ width: '100%' }}>
            <Group justify="space-between" align="center">
                <Title order={2}>Profiles</Title>
                <FileButton onChange={setPendingUploadFile} accept="application/json">
                    {(props) => (
                        <Button {...props} leftSection={<IconUpload size={16} />}>
                            Upload Profile
                        </Button>
                    )}
                </FileButton>
            </Group>

            <TextInput
                placeholder="Search by nickname or profile ID"
                leftSection={<IconSearch size={16} />}
                value={searchString}
                onChange={(e) => setSearchString(e.currentTarget.value)}
            />

            <Box visibleFrom="sm">
                <Paper withBorder p="md">
                    <Table verticalSpacing="sm" highlightOnHover>
                        <Table.Thead>
                            <Table.Tr>
                                <Table.Th>Nickname</Table.Th>
                                <Table.Th>Profile ID</Table.Th>
                                <Table.Th>Level</Table.Th>
                                <Table.Th style={{ textAlign: 'center' }}>Flea Banned</Table.Th>
                                <Table.Th style={{ textAlign: 'right' }}>Actions</Table.Th>
                            </Table.Tr>
                        </Table.Thead>
                        <Table.Tbody>
                            {filteredProfiles.map((row) => (
                                <Table.Tr key={row.profileId}>
                                    <Table.Td fw={600}>{row.nickname}</Table.Td>
                                    <Table.Td>{row.profileId}</Table.Td>
                                    <Table.Td>{row.level}</Table.Td>
                                    <Table.Td style={{ textAlign: 'center' }}>
                                        <Checkbox checked={row.hasFleaBan} readOnly style={{ display: 'inline-block' }} />
                                    </Table.Td>
                                    <Table.Td style={{ textAlign: 'right' }}>
                                        <Group gap="xs" justify="flex-end">
                                            <Button size="xs" leftSection={<IconEdit size={14} />} onClick={() => setSelectedModifyProfile(row)}>
                                                Modify
                                            </Button>
                                            <Tooltip label="WARNING: Can be slow" color="red">
                                                <Button size="xs" leftSection={<IconEye size={14} />} onClick={() => handleViewProfile(row)}>
                                                    View
                                                </Button>
                                            </Tooltip>
                                        </Group>
                                    </Table.Td>
                                </Table.Tr>
                            ))}
                        </Table.Tbody>
                    </Table>
                </Paper>
            </Box>

            <Box hiddenFrom="sm">
                <Stack gap="sm">
                    {filteredProfiles.map((row) => (
                        <Card key={row.profileId} withBorder radius="md" p="md">
                            <Group justify="space-between" mb="xs">
                                <Text fw={700} size="md">
                                    {row.nickname}
                                </Text>
                                <Text size="xs" c="dimmed">
                                    Level {row.level}
                                </Text>
                            </Group>

                            <Text size="xs" c="dimmed" mb="xs">
                                Profile ID: {row.profileId}
                            </Text>

                            <Group justify="space-between" align="center" mb="md">
                                <Text size="sm">Flea Banned:</Text>
                                <Checkbox checked={row.hasFleaBan} readOnly />
                            </Group>

                            <Group grow gap="xs">
                                <Button size="xs" leftSection={<IconEdit size={14} />} onClick={() => setSelectedModifyProfile(row)}>
                                    Modify
                                </Button>
                                <Button size="xs" leftSection={<IconEye size={14} />} onClick={() => handleViewProfile(row)}>
                                    View
                                </Button>
                            </Group>
                        </Card>
                    ))}
                </Stack>
            </Box>

            <ModifyProfileModal profile={selectedModifyProfile} onClose={() => setSelectedModifyProfile(null)} />

            <Modal opened={!!viewJsonData} onClose={() => setViewJsonData(null)} title={viewJsonData?.title} size="xl" centered>
                <ScrollArea h={500}>
                    <CodeHighlight code={viewJsonData?.json || ''} language="json" withCopyButton />
                </ScrollArea>
            </Modal>

            <Modal opened={!!pendingUploadFile} onClose={() => setPendingUploadFile(null)} title="WARNING" centered>
                <Text size="sm" mb="md">
                    This is an experimental feature. Are you sure? Damage might be irreversible!
                </Text>
                <Group justify="flex-end">
                    <Button variant="default" onClick={() => setPendingUploadFile(null)}>
                        NO
                    </Button>
                    <Button color="red" loading={uploadMutation.isPending} onClick={handleFileConfirmUpload}>
                        YES
                    </Button>
                </Group>
            </Modal>
        </Stack>
    );
}
