using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record HeadlessConnectedNotification : IFikaNotification
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.HeadlessConnected;

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
