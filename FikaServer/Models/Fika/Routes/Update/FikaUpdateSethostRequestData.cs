using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Update
{
    public record FikaUpdateSethostRequestData : IRequestData
    {
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
        [JsonPropertyName("ips")]
        public string[] Ips { get; set; } = [];
        [JsonPropertyName("port")]
        public int Port { get; set; }
        [JsonPropertyName("natPunch")]
        public bool NatPunch { get; set; }
        [JsonPropertyName("isHeadless")]
        public bool IsHeadless { get; set; }
    }
}
