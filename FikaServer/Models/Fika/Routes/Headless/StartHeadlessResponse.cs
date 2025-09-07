using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Headless;

public record StartHeadlessResponse
{
    [JsonPropertyName("matchId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? MatchId { get; set; } = null;
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Error { get; set; } = string.Empty;
}
