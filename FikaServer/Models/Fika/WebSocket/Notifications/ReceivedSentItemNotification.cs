using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record ReceivedSentItemNotification : IFikaNotificationBase
    {
        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; } = EFikaNotifications.SentItem;
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;
        [JsonPropertyName("targetId")]
        public string TargetId { get; set; } = string.Empty;
        [JsonPropertyName("itemName")]
        public string ItemName { get; set; } = string.Empty;
    }
}
