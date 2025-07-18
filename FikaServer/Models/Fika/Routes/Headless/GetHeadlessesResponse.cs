using FikaServer.Models.Fika.Headless;
using SPTarkov.Server.Core.Models.Common;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Headless
{
    public record GetHeadlessesResponse
    {
        [JsonPropertyName("headlesses")]
        public Dictionary<MongoId, HeadlessClientInfo> Headlesses { get; set; } = [];
    }
}
