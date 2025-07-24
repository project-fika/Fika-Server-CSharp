using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public abstract record BaseSendItemRequest
    {
        [JsonPropertyName("itemTpl")]
        public required string ItemTemplate { get; set; }

        [JsonPropertyName("amount")]
        public required int Amount { get; set; }

        [JsonPropertyName("message")]
        [MaxLength(255)]
        public required string Message { get; set; }

        [JsonPropertyName("fir")]
        public required bool FoundInRaid { get; set; }

        [JsonPropertyName("expirationDays")]
        public required int ExpirationDays { get; set; }

        [JsonIgnore]
        public DateTime? SendDate { get; set; }
    }
}
