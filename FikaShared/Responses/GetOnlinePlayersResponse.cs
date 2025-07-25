using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetOnlinePlayersResponse
    {
        [JsonPropertyName("players")]
        public required List<OnlinePlayer> Players { get; init; }
    }
}
