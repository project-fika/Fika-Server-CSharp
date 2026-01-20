using FikaServer.Models.Servers.Enums;

namespace FikaServer.Models.Servers;

public class NatPunchToken(NatPunchType type, Guid guid)
{
    public NatPunchType Type { get; } = type;
    public Guid Guid { get; } = guid;
}
