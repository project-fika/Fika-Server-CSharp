using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Join
{
    public record FikaRaidJoinResponse
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;
        [JsonPropertyName("crc32")]
        public uint CRC32 { get; set; }
        [JsonPropertyName("raidCode")]
        public string RaidCode { get; set; } = string.Empty;
    }
}
