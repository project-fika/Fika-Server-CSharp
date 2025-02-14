using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Presence
{
    public record FikaRaidPresence
    {
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
        [JsonPropertyName("side")]
        public EFikaSide Side { get; set; }

        [JsonPropertyName("time")]
        public EFikaTime Time { get; set; }
    }
}
