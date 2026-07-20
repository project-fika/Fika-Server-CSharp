using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

namespace FikaServer;

public sealed record FikaModMetadata : IModMetadata, IModBlazorMetadata
{
    public const string FikaVersion = "2.4.0";

    public string Name { get; init; } = "server";
    public string Author { get; init; } = "Fika";
    public List<string>? Contributors { get; init; } = [];
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/project-fika/Fika-Server-CSharp";
    public string License { get; init; } = "CC-BY-NC-SA-4.0";
    public string ModGuid { get; init; } = "Fika";
    public SemanticVersioning.Version Version { get; init; } = new(FikaVersion);
    public SemanticVersioning.Range SptVersion { get; init; } = new(">=4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public string? WWWRootUrl { get; init; } = null;
    public string? HomePage { get; init; } = null;
    public string? HomePageDescription { get; init; } = null;
}