using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications
{
    public record AddBanNotification : WsNotificationEvent
    {
        [JsonPropertyName("banType")]
        public BanType BanType { get; set; }

        [JsonPropertyName("dateTime")]
        public long DateTime { get; set; }
    }
}
