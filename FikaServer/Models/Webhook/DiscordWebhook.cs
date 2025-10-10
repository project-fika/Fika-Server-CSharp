using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FikaServer.Models.Webhook;

public record DiscordWebhook
{
    [SetsRequiredMembers]
    public DiscordWebhook(string username, string avatarURL, string content)
    {
        Username = username;
        AvatarURL = avatarURL;
        Content = content;
    }

    [JsonPropertyName("username")]
    public required string Username { get; set; } = "Fika Server";

    [JsonPropertyName("avatar_url")]
    public required string AvatarURL { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public required string Content { get; set; } = string.Empty;
}
