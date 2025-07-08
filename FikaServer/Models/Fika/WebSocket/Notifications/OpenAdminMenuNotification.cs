using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record OpenAdminMenuNotification : IFikaNotificationBase
    {
        public OpenAdminMenuNotification(bool success)
        {
            Success = success;
        }

        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; } = EFikaNotifications.OpenAdminSettings;
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
