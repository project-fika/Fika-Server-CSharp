using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Common;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Location;

public record FikaRaidResponse
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;
    [JsonPropertyName("hostUsername")]
    public string HostUsername { get; set; } = string.Empty;
    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; }
    [JsonPropertyName("status")]
    public EFikaMatchStatus Status { get; set; }
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;
    [JsonPropertyName("side")]
    public EFikaSide Side { get; set; }
    [JsonPropertyName("time")]
    public EFikaTime Time { get; set; }
    [JsonPropertyName("players")]
    public Dictionary<MongoId, bool> Players { get; set; } = [];
    [JsonPropertyName("isHeadless")]
    public bool IsHeadless { get; set; }
    [JsonPropertyName("headlessRequesterNickname")]
    public string HeadlessRequesterNickname { get; set; } = string.Empty;
}
