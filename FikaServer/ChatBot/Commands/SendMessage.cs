using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Cache;
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
    public partial class SendMessage(ConfigService configService, MailSendService mailSendService,
        NotificationSendHelper sendHelper, SptWebSocketConnectionHandler websocketHandler,
        FikaProfileService fikaProfileService) : IFikaCommand
    {
        [GeneratedRegex("^fika sendmessage (\\w+) (.+)$")]
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
                return $"fika {Command}\nSends a message to a client\nExample: fika sendmessage Nickname hello\nUse all as a nickname to send to everyone (including you).";
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

            string nickname = match.Groups[1].Value;
            string message = match.Groups[2].Value;

            if (nickname == "all")
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"Everyone has been sent the message:\n{message}");
                websocketHandler.SendMessageToAll(new SendMessageNotification(message)
                {
                    EventType = NotificationEventType.tournamentWarning,
                    EventIdentifier = new()
                });

                return new(value);
            }

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{nickname}' has been sent the message:\n{message}.");

            SptProfile? profile = fikaProfileService.GetProfileByName(nickname);
            sendHelper.SendMessage(profile.ProfileInfo.ProfileId.GetValueOrDefault(), new SendMessageNotification(message)
            {
                EventType = NotificationEventType.tournamentWarning,
                EventIdentifier = new()
            });

            return new(value);
        }


    }
}
