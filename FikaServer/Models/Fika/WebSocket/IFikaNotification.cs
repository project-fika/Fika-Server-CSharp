using System.Text.Json.Serialization;
using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.WebSocket;

public interface IFikaNotification
{
    [JsonPropertyName("type")]
    public abstract EFikaNotification Type { get; set; }
}
