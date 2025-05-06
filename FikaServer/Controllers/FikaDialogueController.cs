using FikaServer.Helpers;
using FikaServer.Services.Cache;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Controllers
{
    [Injectable]
    public class FikaDialogueController(PlayerRelationsService playerRelationsService, DialogueController dialogueController,
        ProfileHelper profileHelper, PlayerRelationsHelper playerRelationsHelper, SaveServer saveServer)
    {
        public GetFriendListDataResponse GetFriendsList(string profileId)
        {
            //var botsAndFriends = dialogueController.GetActiveChatBots();
            var botsAndFriends = new List<UserDialogInfo>();

            var friends = playerRelationsHelper.GetFriendsList(profileId);

            foreach (var friend in friends)
            {
                var profile = profileHelper.GetPmcProfile(friend);
                if (profile == null)
                {
                    playerRelationsHelper.RemoveFriend(profileId, friend);
                    continue;
                }

                botsAndFriends.Add(new()
                {
                    Id = profile.Id,
                    Aid = profile.Aid,
                    Info = new()
                    {
                        Nickname = profile.Info.Nickname,
                        Level = profile.Info.Level,
                        Side = profile.Info.Side,
                        MemberCategory = profile.Info.MemberCategory,
                        SelectedMemberCategory = profile.Info.MemberCategory
                    }
                });
            }

            return new()
            {
                Friends = botsAndFriends,
                Ignore = playerRelationsHelper.GetIgnoreList(profileId),
                InIgnoreList = playerRelationsHelper.GetInIgnoreList(profileId)
            };
        }

        public string SendMessage(string profileId, SendMessageRequest sendMessageRequest)
        {
            var profiles = saveServer.GetProfiles();
            if (!profiles.TryGetValue(profileId, out var profile))
            {
                // it's not a player, let SPT handle it
                return dialogueController.SendMessage(profileId, sendMessageRequest);
            }


            return string.Empty;
        }
    }
}
