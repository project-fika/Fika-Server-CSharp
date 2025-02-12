using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika
{
    public record FikaPlayerRelations
    {
        [JsonPropertyName("Friends")]
        public List<string> Friends { get; set; } = [];
        [JsonPropertyName("Ignore")]
        public List<string> Ignore { get; set; } = [];
    }
}
