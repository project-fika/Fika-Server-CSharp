using System.Text.Json.Serialization;

namespace FikaShared
{
    public record AddFleaBanRequest
    {
        [JsonPropertyName("profileId")]
        public string ProfileId { get; set; }
    }
}
