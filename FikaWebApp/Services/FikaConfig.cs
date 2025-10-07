namespace FikaWebApp.Services;

public class FikaConfig
{
    public string? APIKey { get; set; }
    public Uri? BaseUrl { get; set; }
    public int HeartbeatInterval { get; set; } = 5;
}
