using SPTarkov.Server.Core.Models.Eft.Ws;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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
