using System.Text.Json.Serialization;

namespace FikaWebApp.Models
{
    public record FikaConfigClient
    {
        [JsonPropertyName("useBtr")]
        public bool UseBtr { get; set; } = true;

        [JsonPropertyName("friendlyFire")]
        public bool FriendlyFire { get; set; } = true;

        [JsonPropertyName("dynamicVExfils")]
        public bool DynamicVExfils { get; set; } = false;

        [JsonPropertyName("allowFreeCam")]
        public bool AllowFreeCam { get; set; } = false;

        [JsonPropertyName("allowSpectateFreeCam")]
        public bool AllowSpectateFreeCam { get; set; } = false;

        [JsonPropertyName("blacklistedItems")]
        public string[] BlacklistedItems { get; set; } = [];

        [JsonPropertyName("forceSaveOnDeath")]
        public bool ForceSaveOnDeath { get; set; } = false;

        [JsonPropertyName("mods")]
        public FikaConfigClientMods Mods { get; set; } = new();

        [JsonPropertyName("useInertia")]
        public bool UseInertia { get; set; } = true;

        [JsonPropertyName("sharedQuestProgression")]
        public bool SharedQuestProgression { get; set; } = false;

        [JsonPropertyName("canEditRaidSettings")]
        public bool CanEditRaidSettings { get; set; } = true;

        [JsonPropertyName("enableTransits")]
        public bool EnableTransits { get; set; } = true;

        [JsonPropertyName("anyoneCanStartRaid")]
        public bool AnyoneCanStartRaid { get; set; } = false;
    }

    public record FikaConfigClientMods
    {
        [JsonPropertyName("required")]
        public List<string> Required { get; set; } = [];

        [JsonPropertyName("optional")]
        public List<string> Optional { get; set; } = [];
    }
}
