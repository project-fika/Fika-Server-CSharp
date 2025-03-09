using FikaServer.Models.Fika.Headless;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Headless
{
    public record GetHeadlessesResponse
    {
        [JsonPropertyName("headlesses")]
        public Dictionary<string, HeadlessClientInfo> Headlesses { get; set; } = [];
    }
}
