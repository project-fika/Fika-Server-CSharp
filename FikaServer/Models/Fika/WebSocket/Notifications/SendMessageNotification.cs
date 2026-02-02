using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record SendMessageNotification : WsNotificationEvent
{
    [SetsRequiredMembers]
    public SendMessageNotification(string message)
    {
        Message = message;
    }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
