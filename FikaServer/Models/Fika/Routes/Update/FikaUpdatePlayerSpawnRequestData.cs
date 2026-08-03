using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Update;

public record FikaUpdatePlayerSpawnRequestData : IRequestData
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = string.Empty;
    [JsonPropertyName("groupId")]
    public string GroupId { get; set; } = string.Empty;
}
