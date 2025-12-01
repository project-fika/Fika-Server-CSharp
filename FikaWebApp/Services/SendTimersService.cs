using FikaShared.Requests;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FikaWebApp.Services;

public class SendTimersService(ILogger<SendTimersService> logger, HttpClient httpClient)
{
    public Dictionary<Timer, SendItemRequest> Timers { get; } = [];
    public Dictionary<Timer, SendItemToAllRequest> ToAllTimers { get; } = [];

    private readonly Lock _lock = new();

    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private const string _fileName = "storedTimers.json";

    public record TimerSaveData
    {
        [JsonPropertyName("singleRequests")]
        public required Dictionary<long, SendItemRequest> SingleRequests { get; set; }

        [JsonPropertyName("toAllRequests")]
        public required Dictionary<long, SendItemToAllRequest> ToAllRequests { get; set; }
    }

    public async Task Load()
    {
        var filePath = Path.Combine(WebAppConfig.StoredDataPath, _fileName);
        if (!File.Exists(filePath))
        {
            await Save();
        }

        var raw = await File.ReadAllTextAsync(filePath);
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
                    logger.LogWarning("Found expired timer, forcing to send in 1 minute...");
                }
                sendRequest.SendDate = dt;
                AddTimer(sendRequest, dt, false);
            }

            foreach ((var ticks, var sendRequest) in saveData.ToAllRequests)
            {
                var dt = new DateTime(ticks);
                if (dt < DateTime.Now)
                {
                    dt = DateTime.Now.AddMinutes(1);
                    logger.LogWarning("Found expired timer, forcing to send in 1 minute...");
                }
                sendRequest.SendDate = dt;
                AddTimer(sendRequest, dt, false);
            }
        }

        await Save();
    }

    private Task Save()
    {
        lock (_lock)
        {
            TimerSaveData data = new()
            {
                SingleRequests = Timers.Count != 0
                    ? Timers.Values.ToDictionary(r => r.SendDate.GetValueOrDefault().Ticks)
                    : [],
                ToAllRequests = ToAllTimers.Count != 0
                    ? ToAllTimers.Values.ToDictionary(r => r.SendDate.GetValueOrDefault().Ticks)
                    : []
            };

            var serialized = JsonSerializer.Serialize(data, _serializerOptions);
            var filePath = Path.Combine(WebAppConfig.StoredDataPath, _fileName);
            try
            {
                File.WriteAllText(filePath, serialized.AsSpan());
            }
            catch (Exception ex)
            {
                logger.LogError("There was an error saving the timers: {Error}", ex.Message);
            }
        }

        return Task.CompletedTask;
    }

    public void ClearTimers()
    {
        lock (_lock)
        {
            foreach (var timer in Timers.Keys.Concat(ToAllTimers.Keys))
            {
                timer.Dispose();
            }
            Timers.Clear();
            ToAllTimers.Clear();
        }

        Save();
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
                var result = await httpClient.PostAsJsonAsync("fika/api/senditem", request);
                if (!result.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to send item to {ProfileId}", request.ProfileId);
                }
                else
                {
                    logger.LogInformation("Sent item to {ProfileId}", request.ProfileId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending item");
            }
            finally
            {
                lock (_lock)
                {
                    Timers.Remove(timer!);
                    timer!.Dispose();
                }
                await Save();
            }
        }, null, (int)intervalMs, Timeout.Infinite);

        lock (_lock)
        {
            Timers.Add(timer, request);
            if (save)
            {
                Save();
            }
        }

        logger.LogInformation("Added a timer which will send items in {Hours}h {Minutes}m {Seconds}s", (int)delay.TotalHours, delay.Minutes, delay.Seconds);
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
                var result = await httpClient.PostAsJsonAsync("/fika/api/senditemtoall", request);
                if (!result.IsSuccessStatusCode)
                {
                    failed++;
                }

                var message = $"Sent {request.ProfileIds.Length - failed} requests.";
                if (failed > 0)
                {
                    message += $" {failed} failed to send.";
                }
                logger.LogInformation("{Message}", message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending to all");
            }
            finally
            {
                lock (_lock)
                {
                    ToAllTimers.Remove(timer!);
                    timer!.Dispose();
                }
                await Save();
            }
        }, null, (int)intervalMs, Timeout.Infinite);

        lock (_lock)
        {
            ToAllTimers.Add(timer, request);
            if (save)
            {
                Save();
            }
        }

        logger.LogInformation("Added a timer which will send items in {Hours}h {Minutes}m {Seconds}s", (int)delay.TotalHours, delay.Minutes, delay.Seconds);
    }

    public void RemoveTimer(Timer timer)
    {
        lock (_lock)
        {
            if (Timers.TryGetValue(timer, out var request))
            {
                timer.Dispose();
                Timers.Remove(timer);
                logger.LogInformation("Cancelled timer for ProfileId {ProfileId}", request.ProfileId);
                Save();
            }
            else if (ToAllTimers.TryGetValue(timer, out var _))
            {
                timer.Dispose();
                ToAllTimers.Remove(timer);
                logger.LogInformation("Cancelled timer that was queued for everyone");
                Save();
            }
            else
            {
                logger.LogWarning("No timer found");
            }
        }
    }
}
