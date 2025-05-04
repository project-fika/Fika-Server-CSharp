using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.SendItem.AvailableReceivers
{
    public record SendItemAvailableReceiversRequestData : IRequestData
    {
        [JsonPropertyName("id")]
        public string? ID
        {
            get;
            set;
        }
    }
}
