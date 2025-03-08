using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika
{
    public record FikaMatch
    {
        [JsonPropertyName("ips")]
        public List<string> Ips { get; set; } = [];
        [JsonPropertyName("port")]
        public int Port { get; set; }
        [JsonPropertyName("hostUsername")]
        public string HostUsername { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonPropertyName("fikaVersion")]
        public string FikaVersion { get; set; } = string.Empty;
        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;
        [JsonPropertyName("raidSettings")]
        public required RaidSettings RaidSettings { get; set; }
        [JsonPropertyName("locationData")]
        public required LocationBase LocationData { get; set; }
        [JsonPropertyName("status")]
        public EFikaMatchStatus Status { get; set; }
        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
        [JsonPropertyName("players")]
        public Dictionary<string, FikaPlayer> Players { get; set; } = [];
        [JsonPropertyName("side")]
        public EFikaSide Side { get; set; }
        [JsonPropertyName("time")]
        public EFikaTime Time { get; set; }
        [JsonPropertyName("raidCode")]
        public string RaidCode { get; set; } = string.Empty;
        [JsonPropertyName("natPunch")]
        public bool NatPunch { get; set; }
        [JsonPropertyName("isHeadless")]
        public bool IsHeadless { get; set; }
        [JsonPropertyName("raids")]
        public int Raids { get; set; }
    }
}
