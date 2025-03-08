using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Join
{
    public record FikaRaidJoinResponse
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;
        [JsonPropertyName("fikaVersion")]
        public string FikaVersion { get; set; } = string.Empty;
        [JsonPropertyName("raidCode")]
        public string RaidCode { get; set; } = string.Empty;
    }
}
