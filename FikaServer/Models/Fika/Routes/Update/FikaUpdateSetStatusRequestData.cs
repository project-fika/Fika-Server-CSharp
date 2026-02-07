using System.Text.Json.Serialization;
using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.Routes.Update;

public record FikaUpdateSetStatusRequestData : IRequestData
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;
    [JsonPropertyName("status")]
    public EFikaMatchStatus Status { get; set; }
}
