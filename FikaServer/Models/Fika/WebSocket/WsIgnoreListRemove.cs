using System.Text.Json.Serialization;
using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.Models.Fika.WebSocket;

public record WsIgnoreListRemove : WsNotificationEvent
{
    [JsonPropertyName("_id")]
    public required string Id { get; set; }

    [JsonPropertyName("profile")]
    public required FriendData Profile { get; set; }
}
