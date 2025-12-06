using System.Diagnostics;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class IndexPage : IDisposable
{
    [Inject]
    private HeartbeatService HeartbeatService { get; set; } = default!;

    private CancellationTokenSource _cts = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _cts = new CancellationTokenSource();
        _ = ServerMonitoring(_cts.Token);
    }

    private async Task ServerMonitoring(CancellationToken token)
    {
        var process = Process.GetCurrentProcess();

        while (!token.IsCancellationRequested)
        {
            // CPU usage over 1 second
            var startCpu = process.TotalProcessorTime;
            var startTime = DateTime.UtcNow;
            await Task.Delay(1000, token);
            var endCpu = process.TotalProcessorTime;
            var endTime = DateTime.UtcNow;

            var cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;

            CpuText = $"CPU Usage: {cpuUsage:0.00}%";

            var ramUsage = process.WorkingSet64;
            RamText = $"Ram Usage: {BytesToString(ramUsage)}";

            StateHasChanged();
        }
    }

    private Color StatusColor
    {
        get
        {
            return HeartbeatService.IsRunning ? Color.Success : Color.Error;
        }
    }

    private string StatusText
    {
        get
        {
            return HeartbeatService.IsRunning ? "Running" : "Not running";
        }
    }

    public string RamText { get; set; } = "Ram Usage: N/A";

    public string CpuText { get; set; } = "CPU Usage: N/A";

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

    private string LastRefreshMinutes
    {
        get
        {
            var timeSpan = DateTime.Now - HeartbeatService.LastRefresh;
            if (timeSpan.TotalMinutes < 1)
            {
                return "Last update was less than a minute ago";
            }

            return $"Last update was {(int)timeSpan.TotalMinutes} minute(s) ago";
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}