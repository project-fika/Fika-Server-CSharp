using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Headless
{
    public record GetHeadlessRestartAfterAmountOfRaids
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
