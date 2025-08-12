using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public record FikaConfigBackground
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = true;
}
