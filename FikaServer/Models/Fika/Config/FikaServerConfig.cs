using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public record FikaServerConfig
{
    [JsonPropertyName("client")]
    public FikaConfigClient Client { get; set; } = new();

    [JsonPropertyName("server")]
    public FikaConfigServer Server { get; set; } = new();

    [JsonPropertyName("natPunchServer")]
    public FikaConfigNatPunchServer NatPunchServer { get; set; } = new();

    [JsonPropertyName("headless")]
    public FikaConfigHeadless Headless { get; set; } = new();
}
