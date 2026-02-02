using System.Net;

namespace FikaServer.Models.Servers;

public class NatPunchPeer(Guid guid, IPEndPoint internalAddr, IPEndPoint externalAddr)
{
    public Guid Guid { get; set; } = guid;
    public IPEndPoint InternalAddr { get; } = internalAddr;
    public IPEndPoint ExternalAddr { get; } = externalAddr;
    public DateTime LastRequestTime { get; set; } = DateTime.Now;
    public int TotalRequests { get; set; } = 1;
    public bool IsActive(TimeSpan maxTimeSpan)
    {
        return DateTime.Now - LastRequestTime <= maxTimeSpan;
    }

    public void UpdateRequestInfo()
    {
        LastRequestTime = DateTime.Now;
        TotalRequests++;
    }
}
