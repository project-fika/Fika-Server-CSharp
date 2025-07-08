using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer.ChatBot
{
    [Injectable]
    public class FikaChatBotCommands(IEnumerable<IFikaCommand> fikaCommands) : IChatCommand
    {
        protected readonly IDictionary<string, IFikaCommand> _fikaCommands = fikaCommands.ToDictionary(c => c.Command);

        public string GetCommandHelp(string command)
        {
            return _fikaCommands.TryGetValue(command, out IFikaCommand? value) ? value.CommandHelp : string.Empty;
        }

        public string GetCommandPrefix()
        {
            return "fika";
        }

        public List<string> GetCommands()
        {
            return [.. _fikaCommands.Keys];
        }

        public async ValueTask<string> Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
        {
            return await _fikaCommands[command].PerformAction(commandHandler, sessionId, request);
        }
    }
}
