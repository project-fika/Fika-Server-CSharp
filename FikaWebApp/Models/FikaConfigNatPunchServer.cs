using System.Text.Json.Serialization;

namespace FikaWebApp.Models;

public record FikaConfigNatPunchServer
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = false;

    [JsonPropertyName("port")]
    public int Port { get; set; } = 6790;

    [JsonPropertyName("natIntroduceAmount")]
    public int NatIntroduceAmount { get; set; } = 1;
}
