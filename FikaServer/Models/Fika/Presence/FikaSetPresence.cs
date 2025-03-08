using SPTarkov.Server.Core.Models.Utils;
using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Presence
{
    public record FikaSetPresence
    {
        [JsonPropertyName("activity")]
        public EFikaPlayerPresences Activity { get; set; }
        [JsonPropertyName("raidInformation")]
        public FikaRaidPresence? RaidInformation { get; set; }
    }
}
