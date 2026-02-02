using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.SendItem.AvailableReceivers;

public record SendItemAvailableReceiversRequestData : IRequestData
{
    [JsonPropertyName("id")]
    public string? ID
    {
        get;
        set;
    }
}
