using System.Text.Json.Serialization;

namespace FikaWebApp.Models;

public record FikaConfig
{
    [JsonPropertyName("client")]
    public FikaConfigClient Client { get; set; } = new();

    [JsonPropertyName("server")]
    public FikaConfigServer Server { get; set; } = new();

    [JsonPropertyName("natPunchServer")]
    public FikaConfigNatPunchServer NatPunchServer { get; set; } = new();

    [JsonPropertyName("headless")]
    public FikaConfigHeadless Headless { get; set; } = new();

    [JsonPropertyName("background")]
    public FikaConfigBackground Background { get; set; } = new();


}
