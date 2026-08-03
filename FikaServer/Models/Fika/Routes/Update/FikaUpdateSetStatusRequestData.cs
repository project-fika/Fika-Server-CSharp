using FikaServer.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Update;

public record FikaUpdateSetStatusRequestData : IRequestData
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;
    [JsonPropertyName("status")]
    public EFikaMatchStatus Status { get; set; }
}
