using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.Routes.Headless;

public record StartHeadlessRequest : IRequestData
{
    [JsonPropertyName("headlessSessionID")]
    public string HeadlessSessionID { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public DateTimeEnum Time { get; set; }

    [JsonPropertyName("locationId")]
    public string LocationId { get; set; } = string.Empty;

    [JsonPropertyName("spawnPlace")]
    public PlayersSpawnPlace SpawnPlace { get; set; }

    [JsonPropertyName("metabolismDisabled")]
    public bool MetabolismDisabled { get; set; }

    [JsonPropertyName("timeAndWeatherSettings")]
    public TimeAndWeatherSettings? TimeAndWeatherSettings { get; set; }

    [JsonPropertyName("botSettings")]
    public BotSettings? BotSettings { get; set; }

    [JsonPropertyName("wavesSettings")]
    public WavesSettings? WavesSettings { get; set; }

    [JsonPropertyName("side")]
    public SideType Side { get; set; }

    [JsonPropertyName("customWeather")]
    public bool CustomWeather { get; set; }

    [JsonPropertyName("useEvent")]
    public bool UseEvent { get; set; }
}
