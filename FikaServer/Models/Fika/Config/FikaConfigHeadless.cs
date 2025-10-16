using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public record FikaConfigHeadless
{
    [JsonPropertyName("profiles")]
    public FikaConfigHeadlessProfiles Profiles { get; set; } = new();

    [JsonPropertyName("scripts")]
    public FikaConfigHeadlessScripts Scripts { get; set; } = new();

    [JsonPropertyName("setLevelToAverageOfLobby")]
    public bool SetLevelToAverageOfLobby { get; set; } = true;

    [JsonPropertyName("restartAfterAmountOfRaids")]
    public int RestartAfterAmountOfRaids { get; set; } = 0;
}

public record FikaConfigHeadlessProfiles
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; } = 0;

    [JsonPropertyName("aliases")]
    public Dictionary<string, string> Aliases { get; set; } = [];
}

public record FikaConfigHeadlessScripts
{
    [JsonPropertyName("generate")]
    public bool Generate { get; set; } = true;

    [JsonPropertyName("forceIp")]
    public string ForceIp { get; set; } = "https://127.0.0.1:6969";
}
