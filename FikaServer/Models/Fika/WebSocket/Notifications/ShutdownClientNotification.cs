using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record ShutdownClientNotification : IFikaNotification
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.ShutdownClient;
}
