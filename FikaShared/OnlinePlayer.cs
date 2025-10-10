using System.Text.Json.Serialization;
using static FikaShared.Enums;

namespace FikaShared;

public record OnlinePlayer
{
    [JsonPropertyName("profileId")]
    public required string ProfileId { get; set; }

    [JsonPropertyName("nickname")]
    public required string Nickname { get; set; }

    [JsonPropertyName("level")]
    public required int Level { get; set; }

    [JsonPropertyName("location")]
    public required EFikaLocation Location { get; set; }
}