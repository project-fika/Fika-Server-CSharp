using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Eft.Ws;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket
{
    public record WsIgnoreListAdd : WsNotificationEvent
    {
        [JsonPropertyName("_id")]
        public required string Id { get; set; }

        [JsonPropertyName("profile")]
        public required FriendData Profile { get; set; }
    }
}
