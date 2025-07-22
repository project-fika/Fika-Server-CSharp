using FikaShared.Requests;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FikaWebApp.Services
{
    public class SendTimersService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SendTimersService> _logger;

        private readonly Dictionary<Timer, SendItemRequest> _timers = [];
        private readonly Dictionary<Timer, SendItemToAllRequest> _toAllTimers = [];
        private readonly Lock _lock = new();

        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private const string _dataFolder = "StoredData";
        private const string _fileName = "storedTimers.json";

        public SendTimersService(ILogger<SendTimersService> logger, HttpClient client)
        {
            _httpClient = client;
            _logger = logger;

            Load();
        }

        private record TimerSaveData
        {
            [JsonPropertyName("singleRequests")]
            public required Dictionary<long, SendItemRequest> SingleRequests { get; set; }

            [JsonPropertyName("toAllRequests")]
            public required Dictionary<long, SendItemToAllRequest> ToAllRequests { get; set; }
        }

        private Task Load()
        {
            var filePath = Path.Combine(_dataFolder, _fileName);
            if (!File.Exists(filePath))
            {
                Save();
            }

            var raw = File.ReadAllText(filePath);
            if (raw != null)
            {
                var saveData = JsonSerializer.Deserialize<TimerSaveData>(raw, _serializerOptions);
                if (saveData == null)
                {
                    throw new Exception("Failed to deserialize the timer data!");
                }

                foreach ((var ticks, var sendRequest) in saveData.SingleRequests)
                {
                    var dt = new DateTime(ticks);
                    if (dt < DateTime.Now)
                    {
                        dt = DateTime.Now.AddMinutes(1);
                        _logger.LogWarning("Found expired timer, forcing to send in 1 minute...");
                    }
                    AddTimer(sendRequest, dt, false);
                }

                foreach ((var ticks, var sendRequest) in saveData.ToAllRequests)
                {
                    var dt = new DateTime(ticks);
                    if (dt < DateTime.Now)
                    {
                        dt = DateTime.Now.AddMinutes(1);
                        _logger.LogWarning("Found expired timer, forcing to send in 1 minute...");
                    }
                    AddTimer(sendRequest, dt, false);
                }
            }

            Save();
            return Task.CompletedTask;
        }

        private Task Save()
        {
            lock (_lock)
            {
                TimerSaveData data = new()
                {
                    SingleRequests = _timers.Count != 0
                        ? _timers.Values.ToDictionary(r => r.SendDate.GetValueOrDefault().Ticks)
                        : [],
                    ToAllRequests = _toAllTimers.Count != 0
                        ? _toAllTimers.Values.ToDictionary(r => r.SendDate.GetValueOrDefault().Ticks)
                        : []
                };

                var serialized = JsonSerializer.Serialize(data, _serializerOptions);
                var filePath = Path.Combine(_dataFolder, _fileName);
                try
                {
                    File.WriteAllText(filePath, serialized.AsSpan());
                }
                catch (Exception ex)
                {
                    _logger.LogError("There was an error saving the timers: {Error}", ex.Message);
                }
            }

            return Task.CompletedTask;
        }

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

        public void AddTimer(SendItemRequest request, DateTime targetTime, bool save = true)
        {
            var delay = targetTime - DateTime.Now;
            var intervalMs = Math.Max(delay.TotalMilliseconds, 0d);

            Timer? timer = null;
            request.SendDate = targetTime;

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
                    await Save();
                }
            }, null, (int)intervalMs, Timeout.Infinite);

            lock (_lock)
            {
                _timers.Add(timer, request);
                if (save)
                {
                    Save(); 
                }
            }

            _logger.LogInformation($"Added a timer which will send items in {(int)delay.TotalHours}h {delay.Minutes}m {delay.Seconds}s");
        }

        public void AddTimer(SendItemToAllRequest request, DateTime targetTime, bool save = true)
        {
            var delay = targetTime - DateTime.Now;
            var intervalMs = Math.Max(delay.TotalMilliseconds, 0d);

            Timer? timer = null;
            request.SendDate = targetTime;

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
                    await Save();
                }
            }, null, (int)intervalMs, Timeout.Infinite);

            lock (_lock)
            {
                _toAllTimers.Add(timer, request);
                if (save)
                {
                    Save();
                }
            }

            _logger.LogInformation($"Added a timer which will send items in {(int)delay.TotalHours}h {delay.Minutes}m {delay.Seconds}s");
        }
    }
}
