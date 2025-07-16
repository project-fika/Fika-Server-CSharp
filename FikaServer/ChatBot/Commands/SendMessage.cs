using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Services;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class SendMessage(ConfigService configService, MailSendService mailSendService,
        NotificationSendHelper sendHelper) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+sendmessage\\s+(?:[a-f\\d]{24}|all)\\s+(.*)$")]
        private static partial Regex SendMessageCommandRegex();

        public string Command
        {
            get
            {
                return "sendmessage";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nSends a message to a client\nExample: fika sendmessage 686e0d60baa8bb63cee3dbc3 hello\nUse all to send to everyone (including you).";
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
            Match match = SendMessageCommandRegex().Match(text);
            if (!match.Success)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return new(value);
            }

            string[] split = text.Split(' ');
            string profileId = split[2];
            string message = match.Groups[1].Value;

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' has been sent the message:\n{message}.");

            sendHelper.SendMessage(profileId, new SendMessageNotification(message)
            {
                EventType = NotificationEventType.tournamentWarning,
                EventIdentifier = new()
            });

            return new(value);
        }


    }
}
