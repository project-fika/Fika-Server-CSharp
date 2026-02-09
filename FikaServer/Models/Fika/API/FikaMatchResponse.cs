using System.Text.Json.Serialization;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Routes.Client;

namespace FikaServer.Models.Fika.API;

public record FikaMatchResponse
{
    public FikaMatchResponse(FikaMatch match)
    {
        Ips = match.Ips;
        ServerGuid = match.ServerGuid;
        Port = match.Port;
        HostUsername = match.HostUsername;
        Timestamp = match.Timestamp;
        CRC32 = match.CRC32;
        GameVersion = match.GameVersion;
        Status = match.Status;
        Timeout = match.Timeout;
        Players = [.. match.Players.Keys.Select(x => x.ToString())];
        Side = match.Side;
        Time = match.Time;
        RaidCode = match.RaidCode;
        NatPunch = match.NatPunch;
        UseFikaNatPunchServer = match.UseFikaNatPunchServer;
        IsHeadless = match.IsHeadless;
        Raids = match.Raids;
        CustomRaidSettings = match.CustomRaidSettings;
    }

    [JsonPropertyName("ips")]
    public string[] Ips { get; init; }

    [JsonPropertyName("serverGuid")]
    public Guid ServerGuid { get; init; }

    [JsonPropertyName("port")]
    public ushort Port { get; init; }

    [JsonPropertyName("hostUsername")]
    public string HostUsername { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("crc32")]
    public uint CRC32 { get; init; }

    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; init; }

    [JsonPropertyName("status")]
    public EFikaMatchStatus Status { get; init; }

    [JsonPropertyName("timeout")]
    public int Timeout { get; init; }

    [JsonPropertyName("players")]
    public List<string> Players { get; init; }

    [JsonPropertyName("side")]
    public EFikaSide Side { get; init; }

    [JsonPropertyName("time")]
    public EFikaTime Time { get; init; }

    [JsonPropertyName("raidCode")]
    public string RaidCode { get; init; }

    [JsonPropertyName("natPunch")]
    public bool NatPunch { get; init; }

    [JsonPropertyName("useFikaNatPunchServer")]
    public bool UseFikaNatPunchServer { get; init; }

    [JsonPropertyName("isHeadless")]
    public bool IsHeadless { get; init; }

    [JsonPropertyName("raids")]
    public int Raids { get; init; }

    [JsonPropertyName("customRaidSettings")]
    public FikaCustomRaidSettings? CustomRaidSettings { get; init; }
}