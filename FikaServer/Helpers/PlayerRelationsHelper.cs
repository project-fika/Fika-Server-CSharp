using FikaServer.Models.Fika;
using FikaServer.Models.Fika.WebSocket;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;

namespace FikaServer.Helpers
{
    [Injectable]
    public class PlayerRelationsHelper(ISptLogger<PlayerRelationsHelper> logger,
        PlayerRelationsService playerRelationsService, SaveServer saveServer,
        SptWebSocketConnectionHandler webSocketHandler)
    {
        public List<string> GetFriendsList(string profileId)
        {
            return playerRelationsService.GetStoredValue(profileId).Friends;
        }

        public void RemoveFriend(string fromProfileId, string toProfileId)
        {
            // TODO: Add
            var requesterRelation = playerRelationsService.GetStoredValue(fromProfileId);
            if (requesterRelation == null)
            {
                logger.Debug($"Could not find relations for {fromProfileId}");
                return;
            }

            var friendRelation = playerRelationsService.GetStoredValue(toProfileId);
            if (friendRelation == null)
            {
                logger.Debug($"Could not find relations for target {toProfileId}");
                return;
            }

            if (!requesterRelation.Friends.Remove(toProfileId))
            {
                logger.Warning($"{fromProfileId} tried to remove {toProfileId} from their friend list unsuccessfully");
            }

            if (!friendRelation.Friends.Remove(toProfileId))
            {
                logger.Warning($"{toProfileId} tried to remove {fromProfileId} from their friend list unsuccessfully");
            }

            var profile = saveServer.GetProfile(fromProfileId);
            if (profile != null)
            {
                webSocketHandler.SendMessage(toProfileId, new WsFriendListRemove()
                {
                    EventIdentifier = "youAreRemovedFromFriendList",
                    EventType = NotificationEventType.youAreRemovedFromFriendList,
                    Profile = new()
                    {
                        Aid = profile.ProfileInfo.Aid,
                        Id = profile.ProfileInfo.ProfileId,
                        Info = new()
                        {
                            Level = profile.CharacterData.PmcData.Info.Level,
                            MemberCategory = profile.CharacterData.PmcData.Info.MemberCategory,
                            SelectedMemberCategory = profile.CharacterData.PmcData.Info.SelectedMemberCategory,
                            Nickname = profile.CharacterData.PmcData.Info.Nickname,
                            Side = profile.CharacterData.PmcData.Info.Side
                        }
                    }
                });
            }

            throw new NotImplementedException();
        }

        public List<string> GetIgnoreList(string profileId)
        {
            // TODO: Add
            throw new NotImplementedException();
        }

        public List<string> GetInIgnoreList(string profileId)
        {
            // TODO: Add
            throw new NotImplementedException();
        }
    }
}
