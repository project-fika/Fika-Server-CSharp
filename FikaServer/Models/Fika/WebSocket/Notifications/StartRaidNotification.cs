using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record StartRaidNotification : IFikaNotificationBase
    {
        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; } = EFikaNotifications.StartedRaid;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("isHeadlessRaid")]
        public bool IsHeadlessRaid { get; set; }

        [JsonPropertyName("headlessRequesterName")]
        public string HeadlessRequesterName { get; set; } = string.Empty;

        [JsonPropertyName("raidTime")]
        public EFikaTime RaidTime { get; set; }
    }
}
