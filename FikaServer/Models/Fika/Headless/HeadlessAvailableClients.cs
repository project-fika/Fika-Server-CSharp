using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Models.Fika.Headless;

public record HeadlessAvailableClients
{
    public HeadlessAvailableClients(MongoId sessionId, string alias)
    {
        HeadlessSessionID = sessionId;
        Alias = alias;
    }

    /// <summary>
    /// SessionID of the headless client
    /// </summary>
    [JsonPropertyName("headlessSessionID")]
    public MongoId HeadlessSessionID { get; set; } = default;
    /// <summary>
    /// The alias of the headless client, if it has any assigned. If it doesn't have any assigned uses the nickname 
    /// </summary>
    [JsonPropertyName("alias")]
    public string Alias { get; set; } = string.Empty;
}
