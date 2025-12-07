using System.Net;

namespace FikaWebApp.Services;

public class HeartbeatService
{
    public HeartbeatService(HttpClient client, WebAppConfig fikaConfig, ILogger<HeartbeatService> logger)
    {
        _client = client;
        _interval = fikaConfig.HeartbeatInterval;
        _logger = logger;
        Start();
    }

    public bool IsRunning { get; private set; }
    public DateTime LastRefresh { get; private set; } = DateTime.Now;

    public void Stop()
    {
        if (_timer == null)
        {
            return;
        }

        _timer.Dispose();
        _timer = null;
        IsRunning = false;
    }

    public async Task StopAsync()
    {
        if (_timer == null)
        {
            return;
        }

        await _timer.DisposeAsync();
        _timer = null;
        IsRunning = false;
    }

    public void Start()
    {
        if (_timer != null)
        {
            _logger.LogWarning("HeartbeatService already running.");
            return;
        }

        _timer = new(async _ =>
        {
            try
            {
                var result = await _client.GetAsync("fika/api/heartbeat");
                IsRunning = result.IsSuccessStatusCode;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.StatusCode is HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Something went wrong when querying for heartbeat: 403 Forbidden. Are you using the wrong API key?");
                    IsRunning = false;
                    return;
                }

                if (httpEx.StatusCode is HttpStatusCode.NotFound)
                {
                    _logger.LogError("Something went wrong when querying for heartbeat: 404 NotFound. Are you missing the Fika server mod?");
                    IsRunning = false;
                    return;
                }

                _logger.LogError("There was a HttpRequestException caught when when querying for heartbeat: {Exception}", httpEx.Message);
                IsRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong when querying for heartbeat: {Exception}", ex.Message);
                IsRunning = false;
            }

            LastRefresh = DateTime.Now;
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(_interval));
    }

    private readonly ILogger<HeartbeatService> _logger;
    private readonly HttpClient _client;
    private readonly int _interval;
    private Timer? _timer;

}
