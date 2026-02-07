using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Models.Fika.Routes.Client;

public record DownloadProfileResponse : IRequestData
{
    [JsonPropertyName("profile")]
    public required SptProfile Profile { get; set; }

    [JsonPropertyName("modData")]
    public Dictionary<string, string>? ModData { get; set; }
}
