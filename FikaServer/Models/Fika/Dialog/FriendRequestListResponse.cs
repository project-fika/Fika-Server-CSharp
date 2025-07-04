using SPTarkov.Server.Core.Models.Common;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Dialog
{
    public record FriendRequestListResponse
    {
        [JsonPropertyName("_id")]
        public MongoId Id { get; set; }

        [JsonPropertyName("from")]
        public MongoId From { get; set; }

        [JsonPropertyName("to")]
        public MongoId To { get; set; }

        [JsonPropertyName("date")]
        public long Date { get; set; }

        [JsonPropertyName("profile")]
        public FriendData Profile { get; set; }
    }
}
