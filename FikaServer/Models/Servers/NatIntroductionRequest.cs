namespace FikaServer.Models.Servers;

public enum NatIntroductionType
{
    Server,
    Client
}

public class NatIntroductionRequest(NatIntroductionType natIntroductionType, string sessionId)
{
    public NatIntroductionType Type { get; } = natIntroductionType;
    public string SessionId { get; } = sessionId;
}
