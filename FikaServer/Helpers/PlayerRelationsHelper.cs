using FikaServer.Models.Fika;
using FikaServer.Models.Fika.WebSocket;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
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
        /// <summary>
        /// Gets the friends list of a player
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns>Friends list</returns>
        public List<string> GetFriendsList(string profileId)
        {
            return playerRelationsService.GetStoredValue(profileId).Friends;
        }

        /// <summary>
        /// Removes a friend relation
        /// </summary>
        /// <param name="fromProfileId">Requesting profileId</param>
        /// <param name="toProfileId">Target profileId</param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveFriend(string fromProfileId, string toProfileId)
        {
            // TODO: Add
            FikaPlayerRelations requesterRelation = playerRelationsService.GetStoredValue(fromProfileId);
            if (requesterRelation == null)
            {
                logger.Debug($"Could not find relations for {fromProfileId}");
                return;
            }

            FikaPlayerRelations friendRelation = playerRelationsService.GetStoredValue(toProfileId);
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

            SptProfile profile = saveServer.GetProfile(fromProfileId);
            if (profile != null && profile.ProfileInfo != null && profile.CharacterData?.PmcData?.Info != null)
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
        }

        /// <summary>
        /// Gets the ignore list of a player
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns>Ignore list</returns>
        public List<string> GetIgnoreList(string profileId)
        {
            return playerRelationsService.GetStoredValue(profileId).Ignore;
        }

        /// <summary>
        /// Returns a list of players ignoring this player
        /// </summary>
        /// <param name="profileId">The player to check for</param>
        /// <returns>List of players ignoring the player</returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> GetInIgnoreList(string profileId)
        {
            List<string> keys = playerRelationsService.Keys;

            return [.. keys.Where(x => playerRelationsService.GetStoredValue(x).Ignore.Contains(profileId))];
        }
    }
}
