using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaServer.Services.Headless;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.ChatBot.Commands;

[Injectable]
public partial class ShutdownClient(ConfigService configService, MailSendService mailSendService,
    NotificationWebSocket notificationWebSocket, HeadlessService headlessService, JsonUtil jsonUtil,
    FikaProfileService fikaProfileService) : IFikaCommand
{
    [GeneratedRegex("^fika shutdownclient (\\w+)$")]
    private static partial Regex ShutdownClientCommandRegex();

    public string Command
    {
        get
        {
            return "shutdownclient";
        }
    }

    public string CommandHelp
    {
        get
        {
            return $"fika {Command}\nForces a headless client to shutdown\nExample: fika shutdownclient Nickname";
        }
    }

    public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var value = request.DialogId;
        var isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
        if (!isAdmin)
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "You are not an admin!");
            return value;
        }

        var text = request.Text;
        if (!ShutdownClientCommandRegex().IsMatch(text))
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "Invalid use of the command.");
            return value;
        }

        var split = text.Split(' ');
        var nickname = split[2];
        var profile = fikaProfileService.GetProfileByNickname(nickname);

        if (headlessService.HeadlessClients.TryGetValue(profile.ProfileInfo.ProfileId.GetValueOrDefault(), out var client))
        {
            if (client.WebSocket == null || client.WebSocket.State is WebSocketState.Closed)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"'{nickname}' is not connected to the headless websocket, cannot shutdown.");

                return value;
            }

            var data = jsonUtil.Serialize(new HeadlessShutdownClient())
                ?? throw new NullReferenceException("ShutdownClient::Data was null after serializing");
            await client.WebSocket.SendAsync(Encoding.UTF8.GetBytes(data),
            WebSocketMessageType.Text, true, CancellationToken.None);

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"'{nickname}' is shutting down.");

            return value;
        }

        mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"'{nickname}' is shutting down.");
        await notificationWebSocket.SendAsync(profile.ProfileInfo.ProfileId.GetValueOrDefault(), new ShutdownClientNotification());

        return value;
    }
}
