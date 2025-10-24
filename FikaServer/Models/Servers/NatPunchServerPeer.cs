using System.Net;

namespace FikaServer.Models.Servers;

public class NatPunchServerPeer(string sessionId, IPEndPoint internalAddr, IPEndPoint externalAddr)
{
    public string SessionId { get; set; } = sessionId;
    public IPEndPoint InternalAddr { get; } = internalAddr;
    public IPEndPoint ExternalAddr { get; } = externalAddr;
    public DateTime LastKeepAliveTime { get; set;  } = DateTime.UtcNow;
}
