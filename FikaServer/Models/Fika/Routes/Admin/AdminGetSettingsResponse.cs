using FikaServer.Services;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Admin
{
    public record AdminGetSettingsResponse
    {
        public AdminGetSettingsResponse(ConfigService service)
        {
            FriendlyFire = service.Config.Client.FriendlyFire;
            FreeCam = service.Config.Client.AllowFreeCam;
            SpectateFreeCam = service.Config.Client.AllowSpectateFreeCam;
            SharedQuestProgression = service.Config.Client.SharedQuestProgression;
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
}
