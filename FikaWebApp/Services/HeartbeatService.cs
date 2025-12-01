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
