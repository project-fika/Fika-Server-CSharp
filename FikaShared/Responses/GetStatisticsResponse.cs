using System.Text.Json.Serialization;

namespace FikaShared.Responses;

public record GetStatisticsResponse
{
    public required List<StatisticsPlayer> Players { get; init; }
}

public record StatisticsPlayer
{
    [JsonPropertyName("nickname")]
    public required string Nickname { get; init; }

    [JsonPropertyName("kills")]
    public required double Kills { get; init; }

    [JsonPropertyName("deaths")]
    public required double Deaths { get; init; }

    [JsonPropertyName("ammoUsed")]
    public required double AmmoUsed { get; init; }

    [JsonPropertyName("bodyDamage")]
    public required double BodyDamage { get; init; }

    [JsonPropertyName("armorDamage")]
    public required double ArmorDamage { get; init; }

    [JsonPropertyName("headshots")]
    public required double Headshots { get; init; }

    [JsonPropertyName("bossKills")]
    public required double BossKills { get; init; }
}
