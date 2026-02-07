using System.Text.Json.Serialization;
using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record PushNotification : IFikaNotification, IRequestData
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.PushNotification;

    [JsonPropertyName("notificationIcon")]
    public EEFTNotificationIconType NotificationIcon { get; set; } = EEFTNotificationIconType.Default;

    [JsonPropertyName("notification")]
    public string Notification { get; set; } = string.Empty;

}
