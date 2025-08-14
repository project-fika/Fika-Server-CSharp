using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.RaidSettings.TimeAndWeather;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Settings;

public record FikaRaidSettingsResponse
{
    [JsonPropertyName("received")]
    public bool Received { get; set; }

    [JsonPropertyName("metabolismDisabled")]
    public bool MetabolismDisabled { get; set; }

    [JsonPropertyName("playersSpawnPlace")]
    public PlayersSpawnPlace PlayersSpawnPlace { get; set; }

    [JsonPropertyName("hourOfDay")]
    public int HourOfDay { get; set; }

    [JsonPropertyName("timeFlowType")]
    public TimeFlowType TimeFlowType { get; set; }
}
