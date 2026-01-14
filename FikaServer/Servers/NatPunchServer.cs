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
    private readonly Dictionary<Guid, NatPunchServerPeer> _serverPeers = [];
    private NetManager? _netServer;
    private CancellationTokenSource? _pollEventsRoutineCts;
    private DateTime _lastCleanupPeers = DateTime.Now;

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
        if (!TryParseToken(token, out var natPunchIntroduction) || natPunchIntroduction is null)
        {
            logger.Error($"[Fika NatPunch] Invalid token sent by peer: {remoteEndPoint}.");
            return;
        }

        Guid guid = natPunchIntroduction.Guid;

        if (natPunchIntroduction.Type == "Server")
        {
            if (_serverPeers.TryGetValue(guid, out var serverPeer))
            {
                serverPeer.LastRequestTime = DateTime.Now;

                logger.Info($"[Fika NatPunch] Keep Alive {guid} ({remoteEndPoint}).");
            }
            else
            {
                serverPeer = new(guid, localEndPoint, remoteEndPoint);
                _serverPeers[guid] = serverPeer;

                logger.Info($"[Fika NatPunch] Added {guid} ({remoteEndPoint}) to server peers.");
            }
        }

        if (natPunchIntroduction.Type == "Client")
        {
            if (_serverPeers.TryGetValue(guid, out var serverPeer))
            {
                logger.Info($"[Fika NatPunch] Introducing server {guid} to client: {remoteEndPoint}.");

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
                logger.Error($"[Fika NatPunch] Unknown server GUID ({guid}) provided by client: {remoteEndPoint}.");
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

    private void CleanupPeers()
    {
        DateTime currentTime = DateTime.Now;

        if (currentTime - _lastCleanupPeers > TimeSpan.FromSeconds(3))
        {
            List<Guid> serverPeerGuidsToRemove = [];

            foreach (Guid serverPeersGuid in _serverPeers.Keys)
            {
                NatPunchServerPeer serverPeer = _serverPeers[serverPeersGuid];

                if (currentTime - serverPeer.LastRequestTime > TimeSpan.FromSeconds(30))
                {
                    serverPeerGuidsToRemove.Add(serverPeersGuid);
                }
            }

            foreach (Guid serverPeerGuidToRemove in serverPeerGuidsToRemove)
            {
                _serverPeers.Remove(serverPeerGuidToRemove);
                logger.Info($"[Fika NatPunch] Removed {serverPeerGuidToRemove} from server peers.");
            }

            _lastCleanupPeers = currentTime;
        }
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
            CleanupPeers();

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    private bool TryParseToken(string token, out NatPunchIntroduction? natPunchIntroduction)
    {
        natPunchIntroduction = null;

        if (!token.Contains(':'))
        {
            return false;
        }

        string strGuid;
        string type;

        try
        {
            type = token.Split(':')[0];
            strGuid = token.Split(':')[1];
        }
        catch
        {
            return false;
        }

        if (type != "Server" && type != "Client")
        {
            return false;
        }

        if (!Guid.TryParse(strGuid, out var guid))
        {
            return false;
        }

        natPunchIntroduction = new(type, guid);

        return true;
    }
}
