using System.Net;

namespace FikaServer.Models.Servers;

public class NatPunchServerPeer(Guid guid, IPEndPoint internalAddr, IPEndPoint externalAddr)
{
    public Guid Guid { get; set; } = guid;
    public IPEndPoint InternalAddr { get; } = internalAddr;
    public IPEndPoint ExternalAddr { get; } = externalAddr;
    public DateTime LastRequestTime { get; set;  } = DateTime.Now;
}
