using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public record FikaConfigNatPunchServer
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = false;

    [JsonPropertyName("port")]
    public int Port { get; set; } = 6790;
}
