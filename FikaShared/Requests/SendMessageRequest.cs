using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record SendMessageRequest : ProfileIdRequest
    {
        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }
}
