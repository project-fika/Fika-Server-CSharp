using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    /// <summary>
    /// Used to restart the headless, does not run on clients
    /// </summary>
    public record ShutdownClientNotification : IFikaNotificationBase
    {
        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; } = EFikaNotifications.ShutdownClient;
    }
}
