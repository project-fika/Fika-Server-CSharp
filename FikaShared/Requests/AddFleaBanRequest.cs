using System.Text.Json.Serialization;

namespace FikaShared.Requests
{
    public record AddFleaBanRequest : ProfileIdRequest
    {
        [JsonPropertyName("amountOfDays")]
        public required int AmountOfDays { get; init; }
    }
}
