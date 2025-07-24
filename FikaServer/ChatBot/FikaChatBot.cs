using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace FikaServer.ChatBot
{
    [Injectable]
    public class FikaChatBot(ISptLogger<AbstractDialogChatBot> logger, MailSendService mailSendService,
        ServerLocalisationService localisationService, IEnumerable<FikaChatBotCommands> chatCommands)
        : AbstractDialogChatBot(logger, mailSendService, localisationService, chatCommands)
    {
        private readonly Dictionary<string, FikaChatBotCommands> _fikaCommands = chatCommands.ToDictionary(c => c.CommandPrefix);
        private static readonly MongoId _id = new("686d2c4165a0857987a7f1b8");

        public override UserDialogInfo GetChatBot()
        {
            return new UserDialogInfo
            {
                Id = _id,
                Aid = 1337,
                Info = new()
                {
                    Level = 1,
                    MemberCategory = MemberCategory.Emissary,
                    SelectedMemberCategory = MemberCategory.Emissary,
                    Nickname = "Mr. Fika",
                    Side = "Usec",
                },
            };
        }

        protected override string GetUnrecognizedCommandMessage()
        {
            return "Unknown command! Type \"help\" to see available commands.";
        }
    }
}
