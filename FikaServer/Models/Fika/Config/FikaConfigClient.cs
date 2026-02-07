using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public record FikaConfigClient
{
    [JsonPropertyName("useBtr")]
    public bool UseBtr { get; set; } = true;

    [JsonPropertyName("friendlyFire")]
    public bool FriendlyFire { get; set; } = true;

    [JsonPropertyName("dynamicVExfils")]
    public bool DynamicVExfils { get; set; }

    [JsonPropertyName("allowFreeCam")]
    public bool AllowFreeCam { get; set; }

    [JsonPropertyName("allowSpectateFreeCam")]
    public bool AllowSpectateFreeCam { get; set; }

    [JsonPropertyName("blacklistedItems")]
    public string[] BlacklistedItems { get; set; } = [];

    [JsonPropertyName("forceSaveOnDeath")]
    public bool ForceSaveOnDeath { get; set; }

    [JsonPropertyName("mods")]
    public FikaConfigClientMods Mods { get; set; } = new();

    [JsonPropertyName("useInertia")]
    public bool UseInertia { get; set; } = true;

    [JsonPropertyName("sharedQuestProgression")]
    public bool SharedQuestProgression { get; set; }

    [JsonPropertyName("canEditRaidSettings")]
    public bool CanEditRaidSettings { get; set; } = true;

    [JsonPropertyName("enableTransits")]
    public bool EnableTransits { get; set; } = true;

    [JsonPropertyName("anyoneCanStartRaid")]
    public bool AnyoneCanStartRaid { get; set; }

    [JsonPropertyName("allowNamePlates")]
    public bool AllowNamePlates { get; set; } = true;

    [JsonPropertyName("randomLabyrinthSpawns")]
    public bool RandomLabyrinthSpawns { get; set; }
}

public record FikaConfigClientMods
{
    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = [];

    [JsonPropertyName("optional")]
    public List<string> Optional { get; set; } = [];
}
