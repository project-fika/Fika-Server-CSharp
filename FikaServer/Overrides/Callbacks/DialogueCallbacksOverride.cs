using FikaServer.Controllers;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using System.Reflection;

namespace FikaServer.Overrides.Callbacks
{
    public class ListInboxOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.ListInbox));
        }

        [PatchPrefix]
        public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.ListInbox(sessionID);
            return false;
        }
    }

    public class ListOutboxOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.ListOutbox));
        }

        [PatchPrefix]
        public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.ListOutBox(sessionID);
            return false;
        }
    }

    public class SendFriendRequestOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.SendFriendRequest));
        }

        [PatchPrefix]
        public static bool Prefix(string url, FriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.SendFriendRequest(sessionID, request.To.Value);
            return false;
        }
    }

    public class AcceptAllFriendRequestsOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.AcceptAllFriendRequests));
        }

        [PatchPrefix]
        public static bool Prefix(string url, EmptyRequestData _, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.AcceptAllFriendRequests(sessionID);
            return false;
        }
    }

    public class AcceptFriendRequestOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.AcceptFriendRequest));
        }

        [PatchPrefix]
        public static bool Prefix(AcceptFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.AcceptFriendRequest(sessionID, request);
            return false;
        }
    }

    public class DeclineFriendRequestOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.DeclineFriendRequest));
        }

        [PatchPrefix]
        public static bool Prefix(string url, DeclineFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.DeclineFriendRequest(request.ProfileId, sessionID);
            return false;
        }
    }

    public class CancelFriendRequestOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.CancelFriendRequest));
        }

        [PatchPrefix]
        public static bool Prefix(string url, CancelFriendRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            Console.WriteLine("ASD");
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.CancelFriendRequest(sessionID, request.ProfileId);
            return false;
        }
    }

    public class DeleteFriendOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.DeleteFriend));
        }

        [PatchPrefix]
        public static bool Prefix(string url, DeleteFriendRequest request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.DeleteFriend(sessionID, request.FriendId);
            return false;
        }
    }

    public class IgnoreFriendOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.IgnoreFriend));
        }

        [PatchPrefix]
        public static bool Prefix(UIDRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.IgnoreFriend(sessionID, request.Uid);
            return false;
        }
    }

    public class UnIgnoreFriendOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(DialogueCallbacks)
                .GetMethod(nameof(DialogueCallbacks.UnIgnoreFriend));
        }

        [PatchPrefix]
        public static bool Prefix(UIDRequestData request, MongoId sessionID, ref ValueTask<string> __result)
        {
            FikaDialogueController dialogueController = ServiceLocator.ServiceProvider.GetService<FikaDialogueController>()
                ?? throw new NullReferenceException("Could not get DialogueController");

            __result = dialogueController.UnIgnoreFriend(sessionID, request.Uid);
            return false;
        }
    }


}
