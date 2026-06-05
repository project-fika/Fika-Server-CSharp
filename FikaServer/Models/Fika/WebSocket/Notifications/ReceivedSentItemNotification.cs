using System.Text.Json.Serialization;
using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public sealed record ReceivedSentItemNotification : IFikaNotification
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.SentItem;

    [JsonPropertyName("nickname")]
    public required string Nickname { get; set; }

    [JsonPropertyName("targetId")]
    public required string TargetId { get; set; }

    [JsonPropertyName("itemName")]
    public required string ItemName { get; set; }

    [JsonPropertyName("stackCount")]
    public required double StackCount { get; set; }

    [JsonPropertyName("multiple")]
    public required bool Multiple { get; set; }
}
