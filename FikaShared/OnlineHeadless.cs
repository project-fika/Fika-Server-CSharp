using System.Text.Json.Serialization;
using static FikaShared.Enums;

namespace FikaShared;

public record OnlineHeadless
{
    [JsonPropertyName("profileId")]
    public required string ProfileId { get; set; }

    [JsonPropertyName("nickname")]
    public required string Nickname { get; set; }

    [JsonPropertyName("state")]
    public required EHeadlessState State { get; set; }

    [JsonPropertyName("players")]
    public required int Players { get; set; }
}
