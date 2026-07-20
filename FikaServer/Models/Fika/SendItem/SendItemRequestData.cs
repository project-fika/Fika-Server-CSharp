using SPTarkov.Server.Core.Models.Eft.Common.Request;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.SendItem;

public sealed record SendItemRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("itemIds")]
    public string[]? ItemIds
    {
        get;
        set;
    }

    [JsonPropertyName("target")]
    public string? Target
    {
        get;
        set;
    }
}
