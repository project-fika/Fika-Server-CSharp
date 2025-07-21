using System.Text.Json.Serialization;

namespace FikaShared.Responses
{
    public record GetOnlinePlayersResponse
    {
        [JsonPropertyName("players")]
        public List<OnlinePlayer> Players { get; set; }
    }
}
