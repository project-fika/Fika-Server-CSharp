using System.Diagnostics;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class DashboardMetricsController(
    HeartbeatService heartbeatService) : ControllerBase
{
    public sealed record DashboardMetricsDto(
        bool IsRunning,
        string StatusText,
        string LastRefreshMinutes,
        string CpuUsageText,
        string RamUsageText);

    [HttpGet]
    public async Task<IActionResult> GetMetrics()
    {
        var process = Process.GetCurrentProcess();

        var startCpu = process.TotalProcessorTime;
        var startTime = DateTime.UtcNow;
        await Task.Delay(200);
        var endCpu = process.TotalProcessorTime;
        var endTime = DateTime.UtcNow;

        var cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;

        var ramUsage = process.WorkingSet64;

        var timeSpan = DateTime.Now - heartbeatService.LastRefresh;
        var lastRefreshText = timeSpan.TotalMinutes < 1
            ? "Last update was less than a minute ago"
            : $"Last update was {(int)timeSpan.TotalMinutes} minute(s) ago";

        var dto = new DashboardMetricsDto(
            IsRunning: heartbeatService.IsRunning,
            StatusText: heartbeatService.IsRunning ? "Running" : "Not running",
            LastRefreshMinutes: lastRefreshText,
            CpuUsageText: $"CPU Usage: {cpuUsage:0.00}%",
            RamUsageText: $"Ram Usage: {BytesToString(ramUsage)}"
        );

        return Ok(dto);
    }

    private static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        if (byteCount == 0)
        {
            return "0 B";
        }

        var place = (int)Math.Floor(Math.Log(byteCount, 1024));
        var num = byteCount / Math.Pow(1024, place);
        return $"{num:0.##} {suf[place]}";
    }
}