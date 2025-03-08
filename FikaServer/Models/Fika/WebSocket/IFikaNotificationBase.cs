using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket
{
    public interface IFikaNotificationBase
    {
        [JsonPropertyName("type")]
        public EFikaNotifications Type { get; set; }
    }
}
