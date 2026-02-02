using System.Text.Json.Serialization;
using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.Models.Fika.WebSocket;

public record WsIgnoreListAdd : WsNotificationEvent
{
    [JsonPropertyName("_id")]
    public required MongoId Id { get; set; }

    [JsonPropertyName("profile")]
    public required FriendData Profile { get; set; }
}
