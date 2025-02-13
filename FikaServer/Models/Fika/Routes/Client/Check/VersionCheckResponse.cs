using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Client.Check
{
    public record VersionCheckResponse
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}
