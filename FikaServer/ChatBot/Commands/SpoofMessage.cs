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
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class SpoofMessage(ConfigService configService, NotificationWebSocket notificationWebSocket, MailSendService mailSendService,
        HashUtil hashUtil) : IFikaCommand
    {
        [GeneratedRegex("^fika\\s+spoofmessage\\s+[a-f\\d]{24}\\s+\"([^\"]+)\"\\s+(.*)$")]
        private static partial Regex SpoofMessageCommandRegex();

        public string Command
        {
            get
            {
                return "spoofmessage";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nSpoofs a message to a client, using a fake account\nExample: fika spoofmessage 686e0d60baa8bb63cee3dbc3 \"Test\" hello\nNote: Fake account name has to be surrounded by quotes";
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
            System.Text.RegularExpressions.Match match = SpoofMessageCommandRegex().Match(text);
            if (!match.Success)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return value;
            }

            string[] split = text.Split(' ');
            string profileId = split[2];
            string user = match.Groups[1].Value;
            string message = match.Groups[2].Value;

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{profileId}' been sent the spoofed message:\n{message}");

            MemberCategory memberCategory = MemberCategory.Default;
            Array values = Enum.GetValues<MemberCategory>();
            if (values.Length > 0)
            {
                memberCategory = (MemberCategory)values?.GetValue(Random.Shared.Next(values.Length));
            }           

            mailSendService.SendUserMessageToPlayer(profileId, new()
            {
                Aid = hashUtil.GenerateAccountId(),
                Id = new(),
                Info = new()
                {
                    Nickname = user,
                    Level = Random.Shared.Next(1, 69),
                    MemberCategory = memberCategory,
                    SelectedMemberCategory = memberCategory,
                    Side = "Usec"
                }
            }, message);

            return value;
        }

        
    }
}
