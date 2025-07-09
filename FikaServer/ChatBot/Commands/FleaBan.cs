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
    public partial class FleaBan(ConfigService configService, NotificationWebSocket notificationWebSocket, MailSendService mailSendService,
        SaveServer saveServer, TimeUtil timeUtil, NotificationSendHelper sendHelper) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+fleaban\\s+[a-f\\d]{24}\\s+\\d+$")]
        private static partial Regex FleaBanCommandRegex();

        public string Command
        {
            get
            {
                return "fleaban";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nBans a player from the flea for X days\nExample: fika fleaban 686e0d60baa8bb63cee3dbc3 7\nUse 0 to ban indefinitely.";
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
            if (!FleaBanCommandRegex().IsMatch(text))
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return value;
            }

            string[] split = text.Split(' ');
            string profileId = split[2];
            int days = int.Parse(split[3]);
            if (days == 0)
            {
                days = 9999;
            }
            SptProfile profile = saveServer.GetProfile(profileId);
            if (profile == null)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    $"Could not find profile '{profileId}'.");
                return value;
            }

            long banTime = timeUtil.GetTimeStampFromNowDays(days);
            profile.CharacterData?.PmcData?.Info?.Bans?.Add(new()
            {
                BanType = BanType.RagFair,
                DateTime = banTime
            });
            await saveServer.SaveProfileAsync(profileId);

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' has been banned from the flea for {days} days.");

            sendHelper.SendMessage(profileId, new AddBanNotification()
            {
                EventType = NotificationEventType.InGameBan,
                EventIdentifier = new(),
                BanType = BanType.RagFair,
                DateTime = banTime
            });

            return value;
        }

        
    }
}
