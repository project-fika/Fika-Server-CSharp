using System.Text.Json.Serialization;
using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.Models.Fika.WebSocket;

public record WsFriendListRemove : WsNotificationEvent
{
    [JsonPropertyName("profile")]
    public required FriendData Profile { get; set; }
}
