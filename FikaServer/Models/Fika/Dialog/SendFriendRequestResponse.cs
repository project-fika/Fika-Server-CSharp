using SPTarkov.Server.Core.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Dialog
{
    public record SendFriendRequestResponse
    {
        [JsonPropertyName("status")]
        public required BackendErrorCodes Status { get; set; }
        [JsonPropertyName("requestId")]
        public required string RequestId { get; set; }
        [JsonPropertyName("retryAfter")]
        public required int RetryAfter { get; set; }
    }
}
