using FikaServer.Controllers;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using System.Reflection;

namespace FikaServer.Overrides.Services;

public class GetFriendListOverride : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueController).GetMethod(nameof(DialogueController.GetFriendList))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, ref GetFriendListDataResponse __result)
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
        return typeof(DialogueController).GetMethod(nameof(DialogueController.SendMessage))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, SendMessageRequest request, ref ValueTask<string> __result)
    {
        FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
            ?? throw new NullReferenceException("Missing FikaDialogueController");
        SaveServer saveServer = ServiceLocator.ServiceProvider.GetService<SaveServer>()
            ?? throw new NullReferenceException("Missing SaveServer");

        Dictionary<MongoId, SptProfile> profiles = saveServer.GetProfiles();
        if (!profiles.ContainsKey(sessionId) || !profiles.ContainsKey(request.DialogId))
        {
            return true;
        }

        __result = new(dialogueController.SendMessage(sessionId, request, profiles));
        return false;
    }
}
