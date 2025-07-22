using FikaShared.Requests;

namespace FikaWebApp.Services
{
    public class SendTimersService(ILogger<SendTimersService> logger, HttpClient client)
    {
        private readonly HttpClient _httpClient = client;
        private readonly ILogger<SendTimersService> _logger = logger;

        private readonly Dictionary<Timer, SendItemRequest> _timers = [];
        private readonly Dictionary<Timer, SendItemToAllRequest> _toAllTimers = [];
        private readonly Lock _lock = new();

        public void ClearTimers()
        {
            lock (_lock)
            {
                foreach (var timer in _timers.Keys.Concat(_toAllTimers.Keys))
                {
                    timer.Dispose();
                }
                _timers.Clear();
                _toAllTimers.Clear();
            }
        }

        public void AddTimer(SendItemRequest request, DateTime targetTime)
        {
            var delay = targetTime - DateTime.Now;
            var intervalMs = Math.Max(delay.TotalMilliseconds, 0d);

            Timer? timer = null;

            timer = new Timer(async _ =>
            {
                try
                {
                    var result = await _httpClient.PostAsJsonAsync("post/senditem", request);
                    if (!result.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to send item to {request.ProfileId}");
                    }
                    else
                    {
                        _logger.LogInformation($"Sent item to {request.ProfileId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending item");
                }
                finally
                {
                    lock (_lock)
                    {
                        _timers.Remove(timer!);
                        timer!.Dispose();
                    }
                }
            }, null, (int)intervalMs, Timeout.Infinite);

            lock (_lock)
            {
                _timers.Add(timer, request);
            }

            _logger.LogInformation($"Added a timer which will send items in {(int)delay.TotalHours}h {delay.Minutes}m {delay.Seconds}s");
        }

        public void AddTimer(SendItemToAllRequest request, DateTime targetTime)
        {
            var delay = targetTime - DateTime.Now;
            var intervalMs = Math.Max(delay.TotalMilliseconds, 0d);

            Timer? timer = null;

            timer = new Timer(async _ =>
            {
                try
                {
                    int failed = 0;
                    var result = await _httpClient.PostAsJsonAsync("post/senditemtoall", request);
                    if (!result.IsSuccessStatusCode)
                    {
                        failed++;
                    }

                    var message = $"Sent {request.ProfileIds.Length - failed} requests.";
                    if (failed > 0)
                    {
                        message += $" {failed} failed to send.";
                    }
                    _logger.LogInformation(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending to all");
                }
                finally
                {
                    lock (_lock)
                    {
                        _toAllTimers.Remove(timer!);
                        timer!.Dispose();
                    }
                }
            }, null, (int)intervalMs, Timeout.Infinite);

            lock (_lock)
            {
                _toAllTimers.Add(timer, request);
            }

            _logger.LogInformation($"Added a timer which will send items in {(int)delay.TotalHours}h {delay.Minutes}m {delay.Seconds}s");
        }
    }
}
