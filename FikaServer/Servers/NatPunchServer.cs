using Fika.Core.Networking.LiteNetLib;
using FikaServer.Models.Servers;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using static Fika.Core.Networking.LiteNetLib.EventBasedNatPunchListener;

namespace FikaServer.Servers;

[Injectable(InjectionType.Singleton)]
public class NatPunchServer(ConfigService fikaConfig, ISptLogger<NatPunchServer> logger) : INatPunchListener, INetEventListener
{
    private readonly List<NatPunchServerPeer> _serverPeers = [];
    private NetManager? _netServer;
    private CancellationTokenSource? _pollEventsRoutineCts;

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

            _pollEventsRoutineCts = new CancellationTokenSource();
            Task.Run(PollEventsRoutine, _pollEventsRoutineCts.Token);

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
        _pollEventsRoutineCts?.Cancel();
    }

    public void PollEvents()
    {
        _netServer?.NatPunchModule.PollEvents();
    }

    public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
    {
        DateTime dateTimeNow = DateTime.UtcNow;

        NatIntroductionRequest? natIntroductionRequest;

        try
        {
            natIntroductionRequest = GetNatIntroductionRequestFromToken(token);
        }
        catch
        {
            Console.WriteLine($"[Fika NatPunch] Error when parsing token: {token}");
            return;
        }

        if (natIntroductionRequest == null)
        {
            Console.WriteLine($"[Fika NatPunch] Malformed Nat Introduction Request sent by client: {remoteEndPoint}. Token: {token}");
            return;
        }

        NatPunchServerPeer? serverPeer = _serverPeers.FirstOrDefault(peer => peer.SessionId == natIntroductionRequest.SessionId);

        if (natIntroductionRequest.Type == NatIntroductionType.Server)
        {
            if (serverPeer != null)
            {
                serverPeer.LastKeepAliveTime = dateTimeNow;

                Console.WriteLine($"[Fika NatPunch] KeepAlive {natIntroductionRequest.SessionId} ({remoteEndPoint})");
            }
            else
            {
                serverPeer = new(natIntroductionRequest.SessionId, localEndPoint, remoteEndPoint);
                _serverPeers.Add(serverPeer);

                Console.WriteLine($"[Fika NatPunch] Added {serverPeer.SessionId} ({serverPeer.ExternalAddr}) to server list");
            }
        }

        if (natIntroductionRequest.Type == NatIntroductionType.Client)
        {
            if (serverPeer != null)
            {
                Console.WriteLine($"[Fika NatPunch] Introducing server {serverPeer.SessionId} ({serverPeer.ExternalAddr}) to client ({remoteEndPoint})");

                _netServer?.NatPunchModule.NatIntroduce(
                     serverPeer.InternalAddr,
                     serverPeer.ExternalAddr,
                     localEndPoint,
                     remoteEndPoint,
                     token
                     );
            }
            else
            {
                Console.WriteLine($"Unknown sessionId ({natIntroductionRequest.SessionId}) provided by client: {remoteEndPoint}");
            }
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

    private void RemoteInactivePeers()
    {
        DateTime dateTimeNow = DateTime.UtcNow;

        _serverPeers.RemoveAll(peer => dateTimeNow - peer.LastKeepAliveTime > TimeSpan.FromMinutes(1));
    }

    private async Task PollEventsRoutine()
    {
        while (_netServer != null && _pollEventsRoutineCts != null)
        {
            if (_pollEventsRoutineCts.IsCancellationRequested)
            {
                break;
            }

            PollEvents();
            RemoteInactivePeers();

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
    }

    private NatIntroductionRequest? GetNatIntroductionRequestFromToken(string token)
    {
        if (!token.Contains(':'))
        {
            return null;
        }

        string introductionType = token.Split(':')[0];
        string sessionId = token.Split(':')[1];

        if (introductionType != "Client" && introductionType != "Server")
        {
            return null;
        }

        if (sessionId.Length != 24)
        {
            return null;
        }

        if (Enum.TryParse(introductionType, out NatIntroductionType natIntroductionType))
        {
            return new(natIntroductionType, sessionId);
        }

        return null;
    }
}
