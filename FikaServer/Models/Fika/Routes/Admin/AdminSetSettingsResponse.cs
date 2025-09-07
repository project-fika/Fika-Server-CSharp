using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Routes.Admin;

public record AdminSetSettingsResponse
{
    public AdminSetSettingsResponse(bool success)
    {
        Success = success;
    }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
