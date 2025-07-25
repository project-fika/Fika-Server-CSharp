using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetHeadlessResponse
    {
        [JsonPropertyName("headlessClients")]
        public required List<OnlineHeadless> HeadlessClients { get; init; }
    }
}
