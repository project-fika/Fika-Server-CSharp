using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Presence
{
    public record FikaPlayerPresence
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;
        [JsonPropertyName("level")]
        public int Level { get; set; }
        [JsonPropertyName("activity")]
        public EFikaPlayerPresences Activity { get; set; }
        [JsonPropertyName("activityStartedTimestamp")]
        public long ActivityStartedTimestamp { get; set; }
        [JsonPropertyName("raidInformation")]
        public FikaRaidPresence? RaidInformation { get; set; }
    }
}
