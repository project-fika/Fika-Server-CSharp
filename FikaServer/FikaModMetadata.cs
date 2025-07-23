using SPTarkov.Server.Core.Models.Spt.Mod;

namespace FikaServer
{
    public record FikaModMetadata : AbstractModMetadata
    {
        public override string Name { get; init; } = "server";
        public override string Author { get; init; } = "Fika";
        public override List<string>? Contributors { get; set; } = [];
        public override string Version { get; init; } = "3.0.0";
        public override string SptVersion { get; init; } = "4.0.0";
        public override List<string>? LoadBefore { get; set; } = [];
        public override List<string>? LoadAfter { get; set; } = [];
        public override List<string>? Incompatibilities { get; set; } = [];
        public override Dictionary<string, string>? ModDependencies { get; set; } = [];
        public override string? Url { get; set; } = "https://github.com/project-fika/Fika-Server";
        public override bool? IsBundleMod { get; set; } = false;
        public override string License { get; init; } = "CC-BY-NC-SA-4.0";
        public override string ModGuid { get; init; } = "Fika";
    }
}