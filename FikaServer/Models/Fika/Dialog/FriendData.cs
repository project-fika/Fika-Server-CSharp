using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace FikaServer.Models.Fika.Dialog;

public record FriendData
{
    [JsonPropertyName("_id")]
    public required MongoId? Id { get; set; }

    [JsonPropertyName("aid")]
    public required int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public required ChatFriendData? Info { get; set; }
}

public record ChatFriendData
{
    [JsonPropertyName("Nickname")]
    public required string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public required string? Side { get; set; }

    [JsonPropertyName("Level")]
    public required int? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public required MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("SelectedMemberCategory")]
    public required MemberCategory? SelectedMemberCategory { get; set; }

    [JsonPropertyName("Ignored")]
    public bool? Ignored { get; set; }

    [JsonPropertyName("Banned")]
    public bool? Banned { get; set; }
}
