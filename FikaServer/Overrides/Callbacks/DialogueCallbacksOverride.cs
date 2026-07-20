using FikaServer.Controllers;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using System.Reflection;

namespace FikaServer.Overrides.Callbacks;

[Injectable]
public sealed class ListInboxOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public ListInboxOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.ListInbox))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.ListInbox(sessionID);
        return false;
    }
}

[Injectable]
public sealed class ListOutboxOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public ListOutboxOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.ListOutbox))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.ListOutBox(sessionID);
        return false;
    }
}

[Injectable]
public sealed class SendFriendRequestOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public SendFriendRequestOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.SendFriendRequest))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, FriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.SendFriendRequest(sessionID, request.To.Value);
        return false;
    }
}

[Injectable]
public sealed class AcceptAllFriendRequestsOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public AcceptAllFriendRequestsOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.AcceptAllFriendRequests))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.AcceptAllFriendRequests(sessionID);
        return false;
    }
}

[Injectable]
public sealed class AcceptFriendRequestOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public AcceptFriendRequestOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.AcceptFriendRequest))!;
    }

    [PatchPrefix]
    public static bool Prefix(AcceptFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.AcceptFriendRequest(sessionID, request);
        return false;
    }
}

[Injectable]
public sealed class DeclineFriendRequestOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public DeclineFriendRequestOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.DeclineFriendRequest))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, DeclineFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.DeclineFriendRequest(request.ProfileId, sessionID);
        return false;
    }
}

[Injectable]
public sealed class CancelFriendRequestOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public CancelFriendRequestOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.CancelFriendRequest))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, CancelFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.CancelFriendRequest(sessionID, request.ProfileId);
        return false;
    }
}

[Injectable]
public sealed class DeleteFriendOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public DeleteFriendOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.DeleteFriend))!;
    }

    [PatchPrefix]
    public static bool Prefix(string url, DeleteFriendRequest request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.DeleteFriend(sessionID, request.FriendId);
        return false;
    }
}

[Injectable]
public sealed class IgnoreFriendOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public IgnoreFriendOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.IgnoreFriend))!;
    }

    [PatchPrefix]
    public static bool Prefix(UIDRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.IgnoreFriend(sessionID, request.Uid);
        return false;
    }
}

[Injectable]
public sealed class UnIgnoreFriendOverride : AbstractPatch
{
    private static FikaDialogueController _dialogueController = default!;

    public UnIgnoreFriendOverride(FikaDialogueController dialogueController)
    {
        _dialogueController = dialogueController;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(DialogueCallbacks)
            .GetMethod(nameof(DialogueCallbacks.UnIgnoreFriend))!;
    }

    [PatchPrefix]
    public static bool Prefix(UIDRequestData request, MongoId sessionID, ref ValueTask<string> __result)
    {
        __result = _dialogueController.UnIgnoreFriend(sessionID, request.Uid);
        return false;
    }
}
