using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.Routes.Update;

public record FikaUpdatePingRequestData : IRequestData
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;
}
