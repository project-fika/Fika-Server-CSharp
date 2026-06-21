using System.Text.Json.Serialization;
using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Models.Fika.Presence;

public sealed record FikaRaidPresence
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public EFikaSide Side { get; set; }

    [JsonPropertyName("time")]
    public EFikaTime Time { get; set; }

    [JsonPropertyName("started")]
    public bool Started { get; set; }

    [JsonPropertyName("matchId")]
    public MongoId MatchId { get; set; }
}
