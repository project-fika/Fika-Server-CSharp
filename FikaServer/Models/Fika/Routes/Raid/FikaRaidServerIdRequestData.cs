using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid
{
    public record FikaRaidServerIdRequestData : IRequestData
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
    }
}
