using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Client.Check;

public record FikaCheckModResponse
{
    [JsonPropertyName("forbidden")]
    public required List<string> Forbidden { get; set; }
    [JsonPropertyName("missingRequired")]
    public required List<string> MissingRequired { get; set; }
    [JsonPropertyName("hashMismatch")]
    public required List<string> HashMismatch { get; set; }
}
