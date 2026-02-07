using System.Text.Json.Serialization;
using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.WebSocket.Notifications;

public record StartRaidNotification : IFikaNotification
{
    [JsonPropertyName("type")]
    public EFikaNotification Type { get; set; } = EFikaNotification.StartedRaid;

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("isHeadlessRaid")]
    public bool IsHeadlessRaid { get; set; }

    [JsonPropertyName("headlessRequesterName")]
    public string HeadlessRequesterName { get; set; } = string.Empty;

    [JsonPropertyName("raidTime")]
    public EFikaTime RaidTime { get; set; }
}
