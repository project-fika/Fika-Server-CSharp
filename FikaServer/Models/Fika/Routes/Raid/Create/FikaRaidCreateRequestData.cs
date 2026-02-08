using System.Text.Json.Serialization;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Routes.Client;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.Routes.Raid.Create;

public record FikaRaidCreateRequestData : IRequestData
{
    [JsonPropertyName("raidCode")]
    public string RaidCode { get; set; } = string.Empty;

    [JsonPropertyName("serverId")]
    public MongoId ServerId { get; set; } = default;

    [JsonPropertyName("serverGuid")]
    public Guid ServerGuid { get; set; } = default;

    [JsonPropertyName("hostUsername")]
    public string HostUsername { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("settings")]
    public required GetRaidConfigurationRequestData Settings { get; set; }

    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; } = string.Empty;

    [JsonPropertyName("crc32")]
    public uint CRC32 { get; set; }

    [JsonPropertyName("side")]
    public EFikaSide Side { get; set; }

    [JsonPropertyName("time")]
    public EFikaTime Time { get; set; }

    [JsonPropertyName("isSpectator")]
    public bool IsSpectator { get; set; }

    [JsonPropertyName("customRaidSettings")]
    public FikaCustomRaidSettings? CustomRaidSettings { get; set; }
}
