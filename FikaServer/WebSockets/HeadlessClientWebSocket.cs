using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using FikaServer.Helpers;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;

namespace FikaServer.WebSockets;

[Injectable(InjectionType.Singleton)]
public class HeadlessClientWebSocket(HeadlessHelper headlessHelper, HeadlessService headlessService,
    MatchService matchService, ISptLogger<HeadlessClientWebSocket> logger, WebhookService webhookService,
    NotificationWebSocket notificationWebSocket) : IWebSocketConnectionHandler
{
    private readonly ConcurrentDictionary<string, WebSocket> _headlessWebSockets = [];

    public string GetHookUrl()
    {
        return "/fika/headless/client";
    }

    public string GetSocketId()
    {
        return "Fika Headless Client";
    }

    public async Task OnConnection(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader))
        {
            await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, string.Empty, CancellationToken.None);
            return;
        }

        var base64EncodedString = authHeader.Split(' ')[1];
        var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedString));
        var authorization = decodedString.Split(':');
        var userSessionID = authorization[0];

        logger.Debug($"[{GetSocketId()}] User is {userSessionID}");

        if (!headlessHelper.IsHeadlessClient(userSessionID))
        {
            logger.Error($"[{GetSocketId()}] Invalid headless client {userSessionID} tried to authenticate!");
            return;
        }

        _headlessWebSockets.TryAdd(userSessionID, ws);

        if (!string.IsNullOrEmpty(matchService.GetMatchIdByProfile(userSessionID)))
        {
            await matchService.DeleteMatch(userSessionID);
        }

        WebSocket? oldSocket = null;
        var isNewClient = false;

        headlessService.HeadlessClients.AddOrUpdate(userSessionID,
        _ =>
        {
            isNewClient = true;
            return new HeadlessClientInfo(ws, EHeadlessStatus.READY);
        },
        (_, existing) =>
        {
            oldSocket = existing.WebSocket;
            return new HeadlessClientInfo(ws, EHeadlessStatus.READY);
        });

        if (oldSocket != null)
        {
            try
            {
                await oldSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation,
                    "Replaced by new connection", CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.Message}", ex);
            }
        }

        var name = headlessHelper.GetHeadlessNickname(userSessionID);

        await webhookService.SendWebhookMessage($"Headless client {name} has connected");
        await notificationWebSocket.BroadcastAsync(new HeadlessConnectedNotification
        {
            Name = name
        });
        if (!isNewClient)
        {
            logger.Info($"Headless client {name} reconnected and replaced existing session");
        }
    }

    public Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        // Do nothing
        return Task.CompletedTask;
    }

    public Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        var userSessionID = _headlessWebSockets.FirstOrDefault(x => x.Value == ws).Key;

        if (!string.IsNullOrEmpty(userSessionID))
        {
            logger.Debug($"[{GetSocketId()}] Deleting client {userSessionID}");

            headlessService.HeadlessClients.TryRemove(userSessionID, out _);
            _headlessWebSockets.TryRemove(userSessionID, out _);
        }

        return Task.CompletedTask;
    }
}
