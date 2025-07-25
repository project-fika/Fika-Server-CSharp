using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record SendItemToAllRequest : BaseSendItemRequest
    {
        [JsonPropertyName("profileIds")]
        public required string[] ProfileIds { get; init; }
    }
}
