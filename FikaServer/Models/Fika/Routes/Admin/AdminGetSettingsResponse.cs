using FikaServer.Models.Fika.Config;
using FikaServer.Services;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Admin;

public record AdminGetSettingsResponse
{
    public AdminGetSettingsResponse(ConfigService service)
    {
        FikaConfigClient client = service.Config.Client;
        FriendlyFire = client.FriendlyFire;
        FreeCam = client.AllowFreeCam;
        SpectateFreeCam = client.AllowSpectateFreeCam;
        SharedQuestProgression = client.SharedQuestProgression;
        AverageLevel = service.Config.Headless.SetLevelToAverageOfLobby;
    }

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
