using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record PushNotification : IFikaNotificationBase, IRequestData
    {
        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; } = EFikaNotifications.PushNotification;

        [JsonPropertyName("notificationIcon")]
        public EEFTNotificationIconType NotificationIcon { get; set; } = EEFTNotificationIconType.Default;

        [JsonPropertyName("notification")]
        public string Notification { get; set; } = string.Empty;

    }
}
