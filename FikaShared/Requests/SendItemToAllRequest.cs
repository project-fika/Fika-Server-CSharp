using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record SendItemToAllRequest
    {
        [JsonPropertyName("profileIds")]
        public required string[] ProfileIds { get; set; }

        [JsonPropertyName("itemTpl")]
        public required string ItemTemplate { get; set; }

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }

        [JsonPropertyName("message")]
        [MaxLength(255)]
        public required string Message { get; set; }

        [JsonIgnore]
        public DateTime? SendDate { get; set; }
    }
}
