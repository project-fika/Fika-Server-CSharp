using System.Net;

namespace FikaServer.Models.Servers;

public class NatPunchServerPeer
{
    public IPEndPoint InternalAddr { get; }
    public IPEndPoint ExternalAddr { get; }
    public DateTime CreationTime { get; }

    public NatPunchServerPeer(IPEndPoint internalAddr, IPEndPoint externalAddr)
    {
        InternalAddr = internalAddr;
        ExternalAddr = externalAddr;
        CreationTime = DateTime.UtcNow;
    }
}
