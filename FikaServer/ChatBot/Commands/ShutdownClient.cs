using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Headless;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class ShutdownClient(ConfigService configService, MailSendService mailSendService,
        NotificationWebSocket notificationWebSocket, HeadlessService headlessService, JsonUtil jsonUtil) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+shutdownclient\\s+[a-f\\d]{24}$")]
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
                return $"fika {Command}\nForces a headless client to shutdown\nExample: fika shutdownclient 686e0d60baa8bb63cee3dbc3";
            }
        }

        public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            string value = request.DialogId;
            bool isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
            if (!isAdmin)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "You are not an admin!");
                return value;
            }

            string text = request.Text;
            if (!ShutdownClientCommandRegex().IsMatch(text))
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return value;
            }

            string[] split = text.Split(' ');
            string profileId = split[2];

            if (headlessService.HeadlessClients.TryGetValue(profileId, out HeadlessClientInfo? client))
            {
                if (client.WebSocket == null || client.WebSocket.State is WebSocketState.Closed)
                {
                    mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' is not connected to the headless websocket, cannot shutdown.");

                    return value;
                }

                string? data = jsonUtil.Serialize(new HeadlessShutdownClient())
                    ?? throw new NullReferenceException("ShutdownClient::Data was null after serializing");
                await client.WebSocket.SendAsync(Encoding.UTF8.GetBytes(data),
                WebSocketMessageType.Text, true, CancellationToken.None);

                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' is shutting down.");

                return value;
            }

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' is shutting down.");
            await notificationWebSocket.SendAsync(profileId, new ShutdownClientNotification());

            return value;
        }


    }
}
