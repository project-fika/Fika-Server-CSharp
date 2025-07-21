using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record SendItemRequest : ProfileIdRequest
    {
        [JsonPropertyName("itemTpl")]
        public required string ItemTemplate { get; set; }

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }
    }
}
