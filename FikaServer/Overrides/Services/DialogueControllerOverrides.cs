using FikaServer.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Servers;
using System.Reflection;

namespace FikaServer.Overrides.Services;

[Injectable]
public sealed class GetFriendListOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public GetFriendListOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueController)
            .GetMethod(nameof(DialogueController.GetFriendList))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, ref GetFriendListDataResponse __result)
    {
        __result = _dialogueController.GetFriendsList(sessionId);

        return false;
    }
}

[Injectable]
public sealed class SendMessageOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;
    private static SaveServer _saveServer = default!;

    public SendMessageOverride(FikaDialogueController dialogueController, SaveServer saveServer)
    {
        _dialogueController = dialogueController;
        _saveServer = saveServer;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueController)
            .GetMethod(nameof(DialogueController.SendMessage))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, SendMessageRequest request, ref ValueTask<string> __result)
    {
        var profiles = _saveServer.GetProfiles();
        if (!profiles.ContainsKey(sessionId) || !profiles.ContainsKey(request.DialogId))
        {
            return true;
        }

        __result = new(_dialogueController.SendMessage(sessionId, request, profiles));
        return false;
    }
}
