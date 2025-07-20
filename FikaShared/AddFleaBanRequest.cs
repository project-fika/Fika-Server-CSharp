using System.Text.Json.Serialization;

namespace FikaShared
{
    public record AddFleaBanRequest : ProfileIdRequest
    {
        [JsonPropertyName("amountOfDays")]
        public required int AmountOfDays { get; set; }
    }
}
