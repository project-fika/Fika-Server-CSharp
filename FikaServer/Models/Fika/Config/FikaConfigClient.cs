using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Config;

public sealed record FikaConfigClient
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

    [JsonPropertyName("pmcFoundInRaid")]
    public bool PMCFoundInRaid { get; set; }

    [JsonPropertyName("allowSpectateBots")]
    public bool AllowSpectateBots { get; set; }

    [JsonPropertyName("instantLoad")]
    public bool InstantLoad { get; set; }

    [JsonPropertyName("fastLoad")]
    public bool FastLoad { get; set; }

    [JsonPropertyName("reviveConfig")]
    public FikaReviveConfig ReviveConfig { get; set; } = new();
}

public sealed record FikaConfigClientMods
{
    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = [];

    [JsonPropertyName("optional")]
    public List<string> Optional { get; set; } = [];
}

public sealed record FikaReviveConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("headshotKills")]
    public bool HeadshotKills { get; set; }

    [JsonPropertyName("grenadesKills")]
    public bool GrenadesKills { get; set; }

    [JsonPropertyName("allowLooting")]
    public bool AllowLooting { get; set; }

    [JsonPropertyName("maxRevives")]
    public int MaxRevives { get; set; }

    [JsonPropertyName("bleedoutTime")]
    public float BleedoutTime { get; set; } = 60f;

    [JsonPropertyName("reviveTime")]
    public float ReviveTime { get; set; } = 5f;
}
