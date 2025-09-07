using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public abstract record BaseSendItemRequest
    {
        [JsonPropertyName("itemTpl")]
        public required string ItemTemplate { get; init; }

        [JsonPropertyName("amount")]
        public required int Amount { get; init; }

        [JsonPropertyName("message")]
        [MaxLength(255)]
        public required string Message { get; init; }

        [JsonPropertyName("fir")]
        public required bool FoundInRaid { get; init; }

        [JsonPropertyName("expirationDays")]
        public required int ExpirationDays { get; init; }

        [JsonIgnore]
        public DateTime? SendDate { get; set; }
    }
}
