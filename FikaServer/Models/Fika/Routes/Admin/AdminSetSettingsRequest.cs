using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Admin;

public record AdminSetSettingsRequest : IRequestData
{
    [JsonPropertyName("friendlyFire")]
    public bool FriendlyFire { get; set; }
    [JsonPropertyName("freeCam")]
    public bool FreeCam { get; set; }
    [JsonPropertyName("spectateFreeCam")]
    public bool SpectateFreeCam { get; set; }
    [JsonPropertyName("sharedQuestProgression")]
    public bool SharedQuestProgression { get; set; }
    [JsonPropertyName("averageLevel")]
    public bool AverageLevel { get; set; }
}
