using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Create
{
    public record FikaRaidCreateResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
