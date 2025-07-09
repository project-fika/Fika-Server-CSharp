using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record RemoveBanNotification : WsNotificationEvent
    {
        [JsonPropertyName("banType")]
        public BanType BanType { get; set; }
    }
}
