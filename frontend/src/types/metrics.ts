export interface DashboardMetricsDto {
    isRunning: boolean;
    statusText: string;
    lastRefreshMinutes: string;
    cpuUsageText: string;
    ramUsageText: string;
}
