using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Host
{
    public record FikaRaidGethostResponse
    {
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
