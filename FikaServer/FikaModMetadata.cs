using SPTarkov.Server.Core.Models.Spt.Mod;

namespace FikaServer
{
    public record FikaModMetadata : AbstractModMetadata
    {
        public override string Name { get; set; } = "server";
        public override string Author { get; set; } = "Fika";
        public override List<string>? Contributors { get; set; } = [];
        public override string Version { get; set; } = "3.0.0";
        public override string SptVersion { get; set; } = "4.0.0";
        public override List<string>? LoadBefore { get; set; } = [];
        public override List<string>? LoadAfter { get; set; } = [];
        public override List<string>? Incompatibilities { get; set; } = [];
        public override Dictionary<string, string>? ModDependencies { get; set; } = [];
        public override string? Url { get; set; } = "https://github.com/project-fika/Fika-Server";
        public override bool? IsBundleMod { get; set; } = false;
        public override string? Licence { get; set; } = "CC-BY-NC-SA-4.0";
        public override string ModId { get; set; } = "Fika";
    }
}