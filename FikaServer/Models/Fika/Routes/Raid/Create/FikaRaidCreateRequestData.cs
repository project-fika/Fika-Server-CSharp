using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Raid.Create
{
    public record FikaRaidCreateRequestData : IRequestData
    {
        [JsonPropertyName("raidCode")]
        public string RaidCode { get; set; } = string.Empty;
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; } = string.Empty;
        [JsonPropertyName("hostUsername")]
        public string HostUsername { get; set; } = string.Empty;
        //Todo(Archangel): Ulong??
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonPropertyName("settings")]
        public GetRaidConfigurationRequestData? Settings { get; set; }
        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;
        [JsonPropertyName("fikaVersion")]
        public string FikaVersion { get; set; } = string.Empty;
        [JsonPropertyName("side")]
        public EFikaSide Side { get; set; }
        [JsonPropertyName("time")]
        public EFikaTime Time { get; set; }
        [JsonPropertyName("isSpectator")]
        public bool IsSpectator { get; set; }
    }
}
