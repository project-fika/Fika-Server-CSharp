using FikaServer.Controllers;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using System.Reflection;

namespace FikaServer.Overrides.Callbacks
{
    public class DialogueCallbacksOverride
    {
        public class SendFriendRequestOverride : AbstractPatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(DialogueCallbacks)
                    .GetMethod(nameof(DialogueCallbacks.AcceptFriendRequest));
            }

            [PatchPrefix]
            public static bool Prefix(AcceptFriendRequestData request, string sessionID, ref ValueTask<string> __result)
            {
                FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                    ?? throw new NullReferenceException("Could not get DialogueController");

                __result = dialogueController.AcceptFriendRequest(sessionID, request);
                return false;
            }
        }
    }
}
