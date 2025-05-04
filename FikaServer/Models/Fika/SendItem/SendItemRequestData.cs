using SPTarkov.Server.Core.Models.Eft.Common.Request;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.SendItem
{
    public record SendItemRequestData : BaseInteractionRequestData
    {
        [JsonPropertyName("id")]
        public string? ID
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
}
