using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika
{
    public record FikaPlayerRelations
    {
        [JsonPropertyName("friends")]
        public List<string> Friends { get; set; } = [];
        [JsonPropertyName("ignore")]
        public List<string> Ignore { get; set; } = [];
    }
}
