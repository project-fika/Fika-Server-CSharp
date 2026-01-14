namespace FikaServer.Models.Servers;

public class NatPunchIntroduction(string type, Guid guid)
{
    public string Type { get; } = type;
    public Guid Guid { get; } = guid;
}
