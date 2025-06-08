using FikaServer.Controllers;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using System.Reflection;

namespace FikaServer.Overrides.Services
{
    public class GetFriendListOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueController).GetMethod(nameof(DialogueController.GetFriendList));
        }

        [PatchPrefix]
        public static bool Prefix(string sessionId, ref GetFriendListDataResponse __result)
        {
            FikaDialogueController? dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>();

            __result = dialogueController.GetFriendsList(sessionId);

            return false;
        }
    }

    public class SendMessageOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueController).GetMethod(nameof(DialogueController.SendMessage));
        }

        [PatchPrefix]
        public static bool Prefix(string sessionId, SendMessageRequest request, ref string __result)
        {
            FikaDialogueController? dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>();

            __result = dialogueController.SendMessage(sessionId, request);

            return false;
        }
    }
}
