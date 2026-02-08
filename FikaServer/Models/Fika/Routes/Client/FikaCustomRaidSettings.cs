using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Client;

public record FikaCustomRaidSettings
{
    [JsonPropertyName("useCustomWeather")]
    public bool UseCustomWeather { get; set; }

    [JsonPropertyName("disableOverload")]
    public bool DisableOverload { get; set; }

    [JsonPropertyName("disableLegStamina")]
    public bool DisableLegStamina { get; set; }

    [JsonPropertyName("disableArmStamina")]
    public bool DisableArmStamina { get; set; }
}