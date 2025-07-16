using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text.RegularExpressions;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public partial class SpoofMessage(ConfigService configService, MailSendService mailSendService,
        HashUtil hashUtil, FikaProfileService fikaProfileService) : IFikaCommand
    {
        [GeneratedRegex("^fika spoofmessage (\\S+) \"([^\"]+)\" (.+)$")]
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
                return $"fika {Command}\nSpoofs a message to a client, using a fake account\nExample: fika spoofmessage Nickname \"Test\" hello\nNote: Fake account name has to be surrounded by quotes";
            }
        }

        public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            ValueTask<string> value = new(request.DialogId);
            bool isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
            if (!isAdmin)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "You are not an admin!");
                return value;
            }

            string text = request.Text;
            Match match = SpoofMessageCommandRegex().Match(text);
            if (!match.Success)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "Invalid use of the command.");
                return value;
            }

            string nickname = match.Groups[1].Value;
            string user = match.Groups[2].Value;
            string message = match.Groups[3].Value;

            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{nickname}' been sent the spoofed message:\n{message}");

            MemberCategory memberCategory = MemberCategory.Default;
            Array values = Enum.GetValues<MemberCategory>();
            if (values.Length > 0)
            {
                memberCategory = (MemberCategory)values?.GetValue(Random.Shared.Next(values.Length));
            }

            SptProfile? profile = fikaProfileService.GetProfileByName(nickname);
            mailSendService.SendUserMessageToPlayer(profile.ProfileInfo.ProfileId.GetValueOrDefault(), new()
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
