using Fika.Core.Networking.LiteNetLib;
using FikaServer.Models.Servers;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using System.Net;
using System.Net.Sockets;

namespace FikaServer.Servers;

[Injectable(InjectionType.Singleton)]
public class NatPunchServer(ConfigService fikaConfig, ISptLogger<NatPunchServer> logger) : INatPunchListener, INetEventListener
{
    private readonly Dictionary<string, NatPunchServerPeer> _servers = [];
    private NetManager? _netServer;

    public void Start()
    {
        if (_netServer is null)
        {
            _netServer = new(this)
            {
                IPv6Enabled = false,
                NatPunchEnabled = true
            };
        }

        try
        {
            _netServer.Start(fikaConfig.Config.Server.SPT.Http.Ip, "", fikaConfig.Config.NatPunchServer.Port);
            _netServer.NatPunchModule.Init(this);

            logger.Success($"[Fika NatPunch] NatPunchServer started on {fikaConfig.Config.Server.SPT.Http.Ip}:{_netServer.LocalPort}");
        }
        catch (Exception ex)
        {
            logger.Error($"[Fika NatPunch] NatPunchServer failed to start: {ex.Message}");
        }
    }

    public void Stop()
    {
        _netServer?.Stop();
    }

    public void PollEvents()
    {
        _netServer?.PollEvents();
        _netServer?.NatPunchModule.PollEvents();
    }

    public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
    {
        string introductionType;
        string sessionId;
        NatPunchServerPeer sPeer;

        try
        {
            introductionType = token.Split(':')[0];
            sessionId = token.Split(':')[1];
        }
        catch (Exception ex)
        {
            logger.Error($"[Fika NatPunch] Error when parsing NatIntroductionRequest: {ex.Message}");
            return;
        }

        switch (introductionType)
        {
            case "server":
                if (_servers.TryGetValue(sessionId, out sPeer))
                {
                    logger.Info($"[Fika NatPunch] KeepAlive {sessionId} ({remoteEndPoint})");
                }
                else
                {
                    logger.Info($"[Fika NatPunch] Added {sessionId} ({remoteEndPoint}) to server list");
                }

                _servers[sessionId] = new NatPunchServerPeer(localEndPoint, remoteEndPoint);
                break;

            case "client":
                if (_servers.TryGetValue(sessionId, out sPeer))
                {
                    logger.Info($"[Fika NatPunch] Introducing server {sessionId} ({sPeer.ExternalAddr}) to client ({remoteEndPoint})");

                    for (int i = 0; i < fikaConfig.Config.NatPunchServer.NatIntroduceAmount; i++)
                    {
                        _netServer?.NatPunchModule.NatIntroduce(
                            sPeer.InternalAddr,
                            sPeer.ExternalAddr,
                            localEndPoint,
                            remoteEndPoint,
                            token
                            );
                    }
                }
                else
                {
                    logger.Info($"[Fika NatPunch] Unknown ServerId provided by client.");
                }
                break;

            default:
                logger.Info($"[Fika NatPunch] Unknown request received: {token}");
                break;

        }
    }

    public void OnNatIntroductionResponse(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
    {
        // Do nothing
    }

    public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
    {
        // Do nothing
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        // Do nothing
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        // Do nothing
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        // Do nothing
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        // Do nothing
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // Do nothing
    }

    public void OnPeerConnected(NetPeer peer)
    {
        // Do nothing
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Do nothing
    }
}
