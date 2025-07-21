using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetHeadlessResponse
    {
        [JsonPropertyName("headlessClients")]
        public List<OnlineHeadless> HeadlessClients { get; set; }
    }
}
