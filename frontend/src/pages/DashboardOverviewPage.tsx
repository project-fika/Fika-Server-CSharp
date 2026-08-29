import { ColorSwatch, Group, Paper, SimpleGrid, Skeleton, Stack, Text, Title } from '@mantine/core';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/axiosClient';

export interface DashboardMetricsDto {
    isRunning: boolean;
    statusText: string;
    lastRefreshMinutes: string;
    cpuUsageText: string;
    ramUsageText: string;
}

export function DashboardOverviewPage() {
    // Poll backend stats every 2 seconds automatically
    const { data: metrics, isLoading } = useQuery<DashboardMetricsDto>({
        queryKey: ['dashboard-metrics'],
        queryFn: async () => (await api.get<DashboardMetricsDto>('/dashboardmetrics')).data,
        refetchInterval: 2000,
    });

    return (
        <Stack gap="md">
            <Title order={2}>Dashboard</Title>

            <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
                {/* Server Status Card */}
                <Paper withBorder p="lg" radius="md" shadow="xs">
                    <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
                        Server Status
                    </Text>

                    {isLoading || !metrics ? (
                        <Stack gap="xs">
                            <Skeleton height={24} width={120} />
                            <Skeleton height={16} width={200} />
                        </Stack>
                    ) : (
                        <Stack gap="xs">
                            <Group gap="sm">
                                <ColorSwatch color={metrics.isRunning ? 'var(--mantine-color-green-6)' : 'var(--mantine-color-red-6)'} size={14} />
                                <Text fw={700} size="xl">
                                    {metrics.statusText}
                                </Text>
                            </Group>
                            <Text size="sm" c="dimmed">
                                {metrics.lastRefreshMinutes}
                            </Text>
                        </Stack>
                    )}
                </Paper>

                {/* WebApp Stats Card */}
                <Paper withBorder p="lg" radius="md" shadow="xs">
                    <Text size="xs" c="dimmed" tt="uppercase" fw={700} mb="xs">
                        WebApp Stats
                    </Text>

                    {isLoading || !metrics ? (
                        <Stack gap="xs">
                            <Skeleton height={20} width={150} />
                            <Skeleton height={20} width={150} />
                        </Stack>
                    ) : (
                        <Stack gap="xs">
                            <Text fw={600} size="md">
                                {metrics.ramUsageText}
                            </Text>
                            <Text fw={600} size="md">
                                {metrics.cpuUsageText}
                            </Text>
                        </Stack>
                    )}
                </Paper>
            </SimpleGrid>
        </Stack>
    );
}
