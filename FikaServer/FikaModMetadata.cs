using SPTarkov.Server.Core.Models.Spt.Mod;

namespace FikaServer;

public record FikaModMetadata : AbstractModMetadata
{
    public override string Name { get; init; } = "server";
    public override string Author { get; init; } = "Fika";
    public override List<string>? Contributors { get; init; } = [];
    public override List<string>? Incompatibilities { get; init; } = [];
    public override Dictionary<string, SemanticVersioning.Version>? ModDependencies { get; init; } = [];
    public override string? Url { get; init; } = "https://github.com/project-fika/Fika-Server";
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "CC-BY-NC-SA-4.0";
    public override string ModGuid { get; init; } = "Fika";
    public override SemanticVersioning.Version Version { get; init; } = new(4, 0, 0);
    public override SemanticVersioning.Version SptVersion { get; init; } = new(4, 0, 0);
}