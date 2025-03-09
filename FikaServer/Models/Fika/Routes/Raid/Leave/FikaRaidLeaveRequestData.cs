using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Leave
{
    public record FikaRaidLeaveRequestData : IRequestData
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
        [JsonPropertyName("profileId")]
        public string ProfileId { get; set; } = string.Empty;
    }
}
