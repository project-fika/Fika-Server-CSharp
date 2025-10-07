using System.Diagnostics.CodeAnalysis;

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

    public required string Username { get; set; } = "Fika Server";
    public required string AvatarURL { get; set; } = string.Empty;
    public required string Content { get; set; } = string.Empty;
}
