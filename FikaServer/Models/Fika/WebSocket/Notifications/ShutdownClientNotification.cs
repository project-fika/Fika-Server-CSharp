using System.Text.Json.Serialization;
using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record ShutdownClientNotification : IFikaNotification
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.ShutdownClient;
}
