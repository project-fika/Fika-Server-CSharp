namespace FikaServer.Models.Fika;

public record FikaPlayer
{
    public string GroupId { get; set; } = string.Empty;
    public bool IsDead { get; set; }
    public bool IsSpectator { get; set; }
}
