using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class RemoveFleaBan(ConfigService configService, NotificationWebSocket notificationWebSocket, MailSendService mailSendService,
        SaveServer saveServer, TimeUtil timeUtil, NotificationSendHelper sendHelper) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+removefleaban\\s+[a-f\\d]{24}$")]
        private static partial Regex RemoveFleaBanCommandRegex();

        public string Command
        {
            get
            {
                return "removefleaban";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nUnbans a player from the flea\nExample: fika removefleaban 686e0d60baa8bb63cee3dbc3";
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
            if (!RemoveFleaBanCommandRegex().IsMatch(text))
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return value;
            }

            string[] split = text.Split(' ');
            string profileId = split[2];
            SptProfile profile = saveServer.GetProfile(profileId);
            if (profile == null)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    $"Could not find profile '{profileId}'.");
                return value;
            }

            List<Ban>? bans = profile.CharacterData.PmcData.Info.Bans;
            if (bans == null || !bans.Any(b => b.BanType == BanType.RagFair))
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    $"'{profileId}' does not have any flea bans.");
                return value;
            }

            bans.RemoveAll(b => b.BanType == BanType.RagFair);

            await saveServer.SaveProfileAsync(profileId);

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' has been unbanned from the flea.");

            sendHelper.SendMessage(profileId, new RemoveBanNotification()
            {
                EventType = NotificationEventType.InGameUnBan,
                EventIdentifier = new(),
                BanType = BanType.RagFair
            });

            return value;
        }

        
    }
}
