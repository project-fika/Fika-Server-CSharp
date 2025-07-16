using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Services;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class ForceLogout(ConfigService configService, MailSendService mailSendService,
        NotificationSendHelper sendHelper, SptWebSocketConnectionHandler websocketHandler) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+forcelogout\\s+(?:[a-f\\d]{24}|all)$")]
        private static partial Regex ForceLogoutCommandRegex();

        public string Command
        {
            get
            {
                return "forcelogout";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nForces a client to logout if they are in the menu\nExample: fika forcelogout 686e0d60baa8bb63cee3dbc3\nUse all to logout everyone (including you).";
            }
        }

        public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            string value = request.DialogId;
            bool isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
            if (!isAdmin)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "You are not an admin!");
                return new(value);
            }

            string text = request.Text;
            if (!ForceLogoutCommandRegex().IsMatch(text))
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return new(value);
            }

            string[] split = text.Split(' ');
            string profileId = split[2];

            if (profileId == "all")
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "Everyone has been forced to log out.");
                websocketHandler.SendMessageToAll(new WsNotificationEvent()
                {
                    EventType = NotificationEventType.ForceLogout,
                    EventIdentifier = new()
                });

                return new(value);
            }

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' has been forced to log out.");

            sendHelper.SendMessage(profileId, new WsNotificationEvent()
            {
                EventType = NotificationEventType.ForceLogout,
                EventIdentifier = new()
            });

            return new(value);
        }


    }
}
