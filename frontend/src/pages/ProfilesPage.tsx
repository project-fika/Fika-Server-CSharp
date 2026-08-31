import { CodeHighlight } from '@mantine/code-highlight';
import { Box, Button, Card, Checkbox, FileButton, Flex, Group, Loader, LoadingOverlay, Modal, Paper, ScrollArea, SegmentedControl, Stack, Table, Text, TextInput, Title, Tooltip } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { IconChecklist, IconCode, IconEdit, IconEye, IconListTree, IconSearch, IconUpload } from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useMemo, useState } from 'react';
import JsonView from 'react18-json-view';
import 'react18-json-view/src/style.css';
import { api } from '../api/axiosClient';
import { ModifyProfileModal } from '../components/ModifyProfileModal';
import { ProfileQuestsModal } from '../components/ProfileQuestsModal';
import type { ProfileResponse } from '../types/profiles';

export function ProfilesPage() {
    const queryClient = useQueryClient();
    const [searchString, setSearchString] = useState('');

    const [selectedModifyProfile, setSelectedModifyProfile] = useState<ProfileResponse | null>(null);
    const [selectedQuestProfile, setSelectedQuestProfile] = useState<ProfileResponse | null>(null);
    const [viewJsonData, setViewJsonData] = useState<{ title: string; rawJson: string; parsedJson: object } | null>(null);
    const [isViewingProfile, setIsViewingProfile] = useState(false);
    const [viewingProfileTitle, setViewingProfileTitle] = useState('');
    const [viewMode, setViewMode] = useState<'tree' | 'raw'>('tree');
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
            const message = err instanceof AxiosError ? err.response?.data?.message || 'There was an error uploading the profile' : 'There was an error uploading the profile';
            notifications.show({
                color: 'red',
                message,
            });
            setPendingUploadFile(null);
        },
    });

    const handleViewProfile = async (profile: ProfileResponse) => {
        setViewingProfileTitle(`Viewing ${profile.nickname}`);
        setIsViewingProfile(true);

        try {
            const res = await api.get<string | object>(`/profiles/raw?profileId=${encodeURIComponent(profile.profileId)}`);

            requestAnimationFrame(() => {
                let rawJson = '';
                let parsedJson: object = {};

                if (typeof res.data === 'string') {
                    rawJson = res.data;
                    try {
                        parsedJson = JSON.parse(res.data);
                        rawJson = JSON.stringify(parsedJson, null, 2);
                    } catch {
                        parsedJson = { raw: res.data };
                    }
                } else {
                    parsedJson = res.data;
                    rawJson = JSON.stringify(res.data, null, 2);
                }

                setViewJsonData({
                    title: `Viewing ${profile.nickname}`,
                    rawJson,
                    parsedJson,
                });
                setIsViewingProfile(false);
            });
        } catch (err: unknown) {
            setIsViewingProfile(false);
            const message = err instanceof AxiosError ? err.response?.data?.message || 'Failed to load profile data' : 'Failed to load profile data';
            notifications.show({
                color: 'red',
                message,
            });
        }
    };

    const handleCloseViewModal = () => {
        setViewJsonData(null);
        setIsViewingProfile(false);
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

            <TextInput placeholder="Search by nickname or profile ID" leftSection={<IconSearch size={16} />} value={searchString} onChange={(e) => setSearchString(e.currentTarget.value)} />

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
                                            {/* <Button size="xs" leftSection={<IconChecklist size={14} />} onClick={() => setSelectedQuestProfile(row)}>
                                                Quests
                                            </Button> */}
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
                                <Button size="xs" variant="light" leftSection={<IconChecklist size={14} />} onClick={() => setSelectedQuestProfile(row)}>
                                    Quests
                                </Button>
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

            <ProfileQuestsModal profile={selectedQuestProfile} onClose={() => setSelectedQuestProfile(null)} />

            <Modal
                opened={isViewingProfile || !!viewJsonData}
                onClose={handleCloseViewModal}
                title={
                    <Group justify="space-between" align="center" style={{ width: '100%', paddingRight: 16 }} wrap="nowrap">
                        <Text fw={600} truncate>
                            {viewJsonData?.title || viewingProfileTitle}
                        </Text>
                        <SegmentedControl
                            size="xs"
                            value={viewMode}
                            onChange={(val) => setViewMode(val as 'tree' | 'raw')}
                            style={{ flexShrink: 0 }}
                            data={[
                                {
                                    value: 'tree',
                                    label: (
                                        <Flex align="center" gap={6} style={{ whiteSpace: 'nowrap' }}>
                                            <IconListTree size={14} />
                                            <Text size="xs">Tree</Text>
                                        </Flex>
                                    ),
                                },
                                {
                                    value: 'raw',
                                    label: (
                                        <Flex align="center" gap={6} style={{ whiteSpace: 'nowrap' }}>
                                            <IconCode size={14} />
                                            <Text size="xs">Raw</Text>
                                        </Flex>
                                    ),
                                },
                            ]}
                        />
                    </Group>
                }
                size="60%"
                centered
            >
                <Box pos="relative" mih={300}>
                    <LoadingOverlay visible={isViewingProfile} zIndex={1000} overlayProps={{ radius: 'sm', blur: 2 }} loaderProps={{ type: 'dots' }} />
                    <ScrollArea h="calc(80vh - 100px)" mt="md" type="auto">
                        {viewMode === 'tree' ? (
                            <Paper
                                p="sm"
                                radius="sm"
                                withBorder
                                style={{
                                    backgroundColor: '#1e1e1e',
                                    borderColor: 'var(--mantine-color-dark-4)',
                                    minHeight: '100%',
                                }}
                            >
                                <JsonView src={viewJsonData?.parsedJson || {}} collapsed={1} dark displaySize displayArrayIndex theme="vscode" />
                            </Paper>
                        ) : (
                            <CodeHighlight code={viewJsonData?.rawJson || ''} language="json" withCopyButton />
                        )}
                    </ScrollArea>
                </Box>
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
