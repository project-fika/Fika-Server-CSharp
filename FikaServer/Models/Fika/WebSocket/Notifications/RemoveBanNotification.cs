using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record RemoveBanNotification : WsNotificationEvent
{
    [JsonPropertyName("banType")]
    public BanType BanType { get; set; }
}
