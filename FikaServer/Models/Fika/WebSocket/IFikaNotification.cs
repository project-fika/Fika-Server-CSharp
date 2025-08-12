using FikaServer.Models.Enums;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.WebSocket;

public interface IFikaNotification
{
    [JsonPropertyName("type")]
    public abstract EFikaNotification Type { get; set; }
}
