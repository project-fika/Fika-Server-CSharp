using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket
{
    public record WsFriendListRemove : WsNotificationEvent
    {
        [JsonPropertyName("profile")]
        public SearchFriendResponse? Profile
        {
            get;
            set;
        }
    }
}
