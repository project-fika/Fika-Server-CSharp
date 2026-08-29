import { PieChart, type PieChartCell } from '@mantine/charts';
import { Center, Loader, Paper, SimpleGrid, Stack, Text, Title } from '@mantine/core';
import { useQuery } from '@tanstack/react-query';
import { api } from '../api/axiosClient';

export interface StatisticsPlayer {
    nickname: string;
    kills: number;
    deaths: number;
    ammoUsed: number;
    bodyDamage: number;
    armorDamage: number;
    headshots: number;
    bossKills: number;
}

const COLOR_PALETTE = ['blue.6', 'green.6', 'yellow.6', 'orange.6', 'red.6', 'grape.6', 'violet.6'];

interface StatPieChartProps {
    title: string;
    dataKey: keyof Omit<StatisticsPlayer, 'nickname'>;
    players: StatisticsPlayer[];
}

function StatPieChart({ title, dataKey, players }: StatPieChartProps) {
    const chartData: PieChartCell[] = players
        .map((p, index) => ({
            name: p.nickname,
            value: Math.round(Number(p[dataKey]) || 0),
            color: COLOR_PALETTE[index % COLOR_PALETTE.length],
        }))
        .sort((a, b) => b.value - a.value);

    return (
        <Paper withBorder p="md" radius="md" style={{ width: '100%' }}>
            <Text fw={700} size="lg" ta="center" mb="md">
                {title}
            </Text>
            <Center>
                <PieChart
                    data={chartData}
                    withTooltip
                    withLabels
                    withLegend
                    labelsPosition="outside"
                    labelsType="value"
                    size={200}
                    strokeWidth={2}
                    tooltipDataSource="segment"
                    valueFormatter={(value) => value.toLocaleString()}
                />
            </Center>
        </Paper>
    );
}

export function StatisticsPage() {
    const { data: players = [], isLoading } = useQuery<StatisticsPlayer[]>({
        queryKey: ['statistics'],
        queryFn: async () => (await api.get<StatisticsPlayer[]>('/statistics')).data,
    });

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
            <Title order={2}>Player Statistics</Title>

            <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="lg">
                <StatPieChart title="Kills" dataKey="kills" players={players} />
                <StatPieChart title="Deaths" dataKey="deaths" players={players} />
                <StatPieChart title="Ammo Used" dataKey="ammoUsed" players={players} />
                <StatPieChart title="Body Damage" dataKey="bodyDamage" players={players} />
                <StatPieChart title="Armor Damage" dataKey="armorDamage" players={players} />
                <StatPieChart title="Headshots" dataKey="headshots" players={players} />
                <StatPieChart title="Boss Kills" dataKey="bossKills" players={players} />
            </SimpleGrid>
        </Stack>
    );
}
