using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Dialog
{
    public record FriendRequestListResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("date")]
        public long Date { get; set; }
    }
}
