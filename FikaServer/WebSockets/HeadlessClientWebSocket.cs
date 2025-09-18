using FikaServer.Helpers;
using FikaServer.Models.Fika.Headless;
using FikaServer.Services;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.WebSockets;

[Injectable(InjectionType.Singleton)]
public class HeadlessClientWebSocket(HeadlessHelper headlessHelper, HeadlessService headlessService,
    MatchService matchService, ISptLogger<HeadlessClientWebSocket> logger, WebhookService webhookService) : IWebSocketConnectionHandler
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
        string authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader))
        {
            await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "", CancellationToken.None);
            return;
        }

        string base64EncodedString = authHeader.Split(' ')[1];
        string decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedString));
        string[] authorization = decodedString.Split(':');
        string userSessionID = authorization[0];

        logger.Debug($"[{GetSocketId()}] User is {userSessionID}");

        if (!headlessHelper.IsHeadlessClient(userSessionID))
        {
            logger.Error($"[{GetSocketId()}] Invalid headless client {userSessionID} tried to authenticate!");
            return;
        }

        _headlessWebSockets.TryAdd(userSessionID, ws);

        if (!string.IsNullOrEmpty(matchService.GetMatchIdByProfile(userSessionID)))
        {
            matchService.DeleteMatch(userSessionID);
        }

        if (!headlessService.HeadlessClients.TryAdd(userSessionID, new HeadlessClientInfo(ws, Models.Enums.EHeadlessStatus.READY)))
        {
            logger.Error($"failed to add headless {userSessionID} to the headless clients list");
        }
        else
        {
            await webhookService.SendWebhookMessage($"Headless client {userSessionID} has connected");
        }
    }

    public Task OnMessage(byte[] rawData, WebSocketMessageType messageType, WebSocket ws, HttpContext context)
    {
        // Do nothing
        return Task.CompletedTask;
    }

    public Task OnClose(WebSocket ws, HttpContext context, string sessionIdContext)
    {
        string userSessionID = _headlessWebSockets.FirstOrDefault(x => x.Value == ws).Key;

        if (!string.IsNullOrEmpty(userSessionID))
        {
            logger.Debug($"[{GetSocketId()}] Deleting client {userSessionID}");

            headlessService.HeadlessClients.TryRemove(userSessionID, out _);
            _headlessWebSockets.TryRemove(userSessionID, out _);
        }

        return Task.CompletedTask;
    }
}
