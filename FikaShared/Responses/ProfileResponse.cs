using System.Text.Json.Serialization;

namespace FikaShared.Responses;

public record ProfileResponse
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = string.Empty;

    [JsonPropertyName("profileId")]
    public string ProfileId { get; init; } = string.Empty;

    [JsonPropertyName("hasFleaBan")]
    public bool HasFleaBan { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; init; }
}
