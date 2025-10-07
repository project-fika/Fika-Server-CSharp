using System.Text.Json.Serialization;

namespace FikaWebApp.Models;

public record FikaConfigBackground
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = true;
}
