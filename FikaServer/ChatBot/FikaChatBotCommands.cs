using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer.ChatBot
{
    public class FikaChatBotCommands : IChatCommand
    {
        public string GetCommandHelp(string command)
        {
            throw new NotImplementedException();
        }

        public string GetCommandPrefix()
        {
            return "fika";
        }

        public List<string> GetCommands()
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
