import { Box, Button, FileButton, Group, Loader, Modal, NavLink, Paper, Progress, Stack, Text, Title } from '@mantine/core';
import { notifications } from '@mantine/notifications';
import {
    IconChevronRight,
    IconDownload,
    IconFile,
    IconFileCode,
    IconFileTypePdf,
    IconFileZip,
    IconFolder,
    IconInfoCircle,
    IconTrash,
    IconUpload,
} from '@tabler/icons-react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useState } from 'react';
import { api } from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import type { FileTreeNode } from '../types/files';

function getFileIcon(fileName: string, isDirectory: boolean) {
    if (isDirectory) return <IconFolder size={18} color="var(--mantine-color-yellow-5)" />;
    const ext = fileName.slice(((fileName.lastIndexOf('.') - 1) >>> 0) + 2).toLowerCase();
    switch (ext) {
        case 'zip':
        case '7z':
        case 'rar':
            return <IconFileZip size={18} color="var(--mantine-color-orange-5)" />;
        case 'pdf':
            return <IconFileTypePdf size={18} color="var(--mantine-color-red-5)" />;
        case 'json':
            return <IconFileCode size={18} color="var(--mantine-color-blue-5)" />;
        default:
            return <IconFile size={18} color="var(--mantine-color-gray-5)" />;
    }
}

interface TreeRenderProps {
    nodes: FileTreeNode[];
    selectedValue: string | null;
    onSelect: (value: string) => void;
}

function TreeRender({ nodes, selectedValue, onSelect }: TreeRenderProps) {
    return (
        <Stack gap={2}>
            {nodes.map((node) => {
                const isSelected = selectedValue === node.value;
                const children = node.children ?? [];
                const hasChildren = children.length > 0;

                if (node.isDirectory) {
                    return (
                        <NavLink
                            key={node.value}
                            label={
                                <Group justify="space-between" wrap="nowrap" style={{ width: '100%' }}>
                                    <Text size="sm">{node.text}</Text>
                                    {node.endText && (
                                        <Text size="xs" c="dimmed">
                                            {node.endText}
                                        </Text>
                                    )}
                                </Group>
                            }
                            leftSection={getFileIcon(node.text, true)}
                            rightSection={<IconChevronRight size={14} className="mantine-rotate-rtl" />}
                            active={isSelected}
                            onClick={() => onSelect(node.value)}
                        >
                            {hasChildren && (
                                <Box pl="md">
                                    <TreeRender nodes={children} selectedValue={selectedValue} onSelect={onSelect} />
                                </Box>
                            )}
                        </NavLink>
                    );
                }

                return (
                    <NavLink
                        key={node.value}
                        label={
                            <Group justify="space-between" wrap="nowrap" style={{ width: '100%' }}>
                                <Text size="sm">{node.text}</Text>
                                {node.endText && (
                                    <Text size="xs" c="dimmed">
                                        {node.endText}
                                    </Text>
                                )}
                            </Group>
                        }
                        leftSection={getFileIcon(node.text, false)}
                        active={isSelected}
                        onClick={() => onSelect(node.value)}
                    />
                );
            })}
        </Stack>
    );
}

export function FileManagerPage() {
    const { hasRole } = useAuth();
    const queryClient = useQueryClient();

    const [infoExpanded, setInfoExpanded] = useState(false);
    const [selectedValue, setSelectedValue] = useState<string | null>(null);
    const [deleteModalOpened, setDeleteModalOpened] = useState(false);

    const [uploadProgress, setUploadProgress] = useState<number>(0);
    const [uploadFileName, setUploadFileName] = useState<string>('');

    const isAdmin = hasRole('Admin');

    const { data: treeNodes = [], isLoading } = useQuery<FileTreeNode[]>({
        queryKey: ['file-tree'],
        queryFn: async () => (await api.get<FileTreeNode[]>('/filemanager/tree')).data,
    });

    const uploadMutation = useMutation({
        mutationFn: async (files: File[]) => {
            const formData = new FormData();
            for (const file of files) {
                formData.append('files', file);
            }
            setUploadFileName(files.length === 1 ? files[0].name : `${files.length} files`);
            setUploadProgress(0);

            return api.post('/filemanager/upload', formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
                onUploadProgress: (progressEvent) => {
                    if (progressEvent.total) {
                        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                        setUploadProgress(percentCompleted);
                    }
                },
            });
        },
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data?.message || 'Files uploaded successfully' });
            setUploadProgress(0);
            setUploadFileName('');
            queryClient.invalidateQueries({ queryKey: ['file-tree'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || err.message : 'Upload failed';
            notifications.show({ color: 'red', message });
            setUploadProgress(0);
            setUploadFileName('');
        },
    });

    const deleteMutation = useMutation({
        mutationFn: (relativePath: string) => api.post('/filemanager/delete', { relativePath }),
        onSuccess: (res) => {
            notifications.show({ color: 'green', message: res.data?.message || 'File deleted successfully' });
            setSelectedValue(null);
            setDeleteModalOpened(false);
            queryClient.invalidateQueries({ queryKey: ['file-tree'] });
        },
        onError: (err: unknown) => {
            const message = err instanceof AxiosError ? err.response?.data?.message || err.message : 'Delete failed';
            notifications.show({ color: 'red', message });
            setDeleteModalOpened(false);
        },
    });

    const handleDownload = async () => {
        if (!selectedValue) return;

        try {
            const response = await api.get(`/filemanager/download/${encodeURIComponent(selectedValue)}`, {
                responseType: 'blob',
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            const filename = selectedValue.split('/').pop() || 'download';
            link.setAttribute('download', filename);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);
        } catch {
            notifications.show({ color: 'red', message: 'Failed to download file' });
        }
    };

    return (
        <Stack gap="md" style={{ width: '100%', position: 'relative' }}>
            <Title order={2}>File Manager</Title>

            <Paper withBorder p="md" radius="md">
                <Stack gap="sm">
                    <Button
                        size="xs"
                        leftSection={<IconInfoCircle size={14} />}
                        onClick={() => setInfoExpanded((v) => !v)}
                        style={{ alignSelf: 'flex-start' }}
                    >
                        {infoExpanded ? 'Hide Info' : 'Show Info'}
                    </Button>

                    {infoExpanded && (
                        <Text size="sm" c="dimmed">
                            You can download files that have been uploaded by the server admin here.
                            <br />
                            <b>NOTE:</b> Files over 250MB are not recommended.
                        </Text>
                    )}

                    {isLoading ? (
                        <Stack align="center" py="xl">
                            <Loader size="md" />
                        </Stack>
                    ) : (
                        <Box style={{ border: '1px solid var(--mantine-color-dark-4)', borderRadius: 8, padding: 8 }}>
                            {treeNodes.length > 0 ? (
                                <TreeRender nodes={treeNodes} selectedValue={selectedValue} onSelect={(val) => setSelectedValue(val)} />
                            ) : (
                                <Text size="sm" c="dimmed" p="md" ta="center">
                                    No files or directories found.
                                </Text>
                            )}
                        </Box>
                    )}
                </Stack>
            </Paper>

            {uploadMutation.isPending && (
                <Paper withBorder p="md" radius="md">
                    <Stack gap="xs">
                        <Group justify="space-between">
                            <Text size="sm" fw={600}>
                                Uploading {uploadFileName}...
                            </Text>
                            <Text size="sm" fw={600} c="blue">
                                {uploadProgress}%
                            </Text>
                        </Group>
                        <Progress value={uploadProgress} animated size="lg" radius="xl" />
                    </Stack>
                </Paper>
            )}

            <Paper withBorder p="md" radius="md">
                <Group gap="sm" grow wrap="wrap">
                    <Button leftSection={<IconDownload size={16} />} disabled={!selectedValue || uploadMutation.isPending} onClick={handleDownload}>
                        Download
                    </Button>

                    {isAdmin && (
                        <>
                            <FileButton onChange={(files) => files && uploadMutation.mutate(files)} multiple>
                                {(props) => (
                                    <Button {...props} leftSection={<IconUpload size={16} />} loading={uploadMutation.isPending}>
                                        Upload
                                    </Button>
                                )}
                            </FileButton>

                            <Button
                                color="red"
                                leftSection={<IconTrash size={16} />}
                                disabled={!selectedValue || uploadMutation.isPending}
                                onClick={() => setDeleteModalOpened(true)}
                            >
                                Delete
                            </Button>
                        </>
                    )}
                </Group>
            </Paper>

            <Modal opened={deleteModalOpened} onClose={() => setDeleteModalOpened(false)} title="Confirmation" centered>
                <Stack gap="md">
                    <Text size="sm">
                        Are you sure you want to remove <b>'{selectedValue}'</b>?<br />
                        This is permanent!
                    </Text>
                    <Group justify="flex-end">
                        <Button variant="default" onClick={() => setDeleteModalOpened(false)}>
                            No
                        </Button>
                        <Button color="red" loading={deleteMutation.isPending} onClick={() => selectedValue && deleteMutation.mutate(selectedValue)}>
                            Yes
                        </Button>
                    </Group>
                </Stack>
            </Modal>
        </Stack>
    );
}
