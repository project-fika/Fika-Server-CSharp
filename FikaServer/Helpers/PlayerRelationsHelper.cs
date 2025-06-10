using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Dialog;
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
        SptWebSocketConnectionHandler webSocketHandler, FriendRequestsService friendRequestsService)
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
                    Profile = profile.ToFriendData()
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
            return [.. playerRelationsService.Keys
                .Where(x => playerRelationsService.GetStoredValue(x).Ignore.Contains(profileId))];
        }

        public bool RemoveFriendRequest(string from, string? to, ERemoveFriendReason reason)
        {
            if (!friendRequestsService.HasFriendRequest(from, to, out FriendRequestListResponse? response))
            {
                logger.Error($"{from} tried to remove a friend request from {to} but it doesn't exist. Reason: {reason}");
                return false;
            }

            friendRequestsService.DeleteFriendRequest(response);
            switch (reason)
            {
                case ERemoveFriendReason.Accept:
                    {
                        SptProfile profile = saveServer.GetProfile(to);
                        webSocketHandler.SendMessage(from, new WsFriendListRemove()
                        {
                            EventIdentifier = "friendListRequestAccept",
                            EventType = NotificationEventType.friendListRequestAccept,
                            Profile = profile.ToFriendData()
                        });
                    }
                    break;
                case ERemoveFriendReason.Cancel:
                    {
                        SptProfile profile = saveServer.GetProfile(from);
                        webSocketHandler.SendMessage(to, new WsFriendListRemove()
                        {
                            EventIdentifier = "friendListRequestCancel",
                            EventType = NotificationEventType.friendListRequestCancel,
                            Profile = profile.ToFriendData()
                        });
                    }
                    break;
                case ERemoveFriendReason.Decline:
                    {
                        SptProfile profile = saveServer.GetProfile(from);
                        webSocketHandler.SendMessage(to, new WsFriendListRemove()
                        {
                            EventIdentifier = "friendListRequestDecline",
                            EventType = NotificationEventType.friendListRequestDecline,
                            Profile = profile.ToFriendData()
                        });
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Adds new relations to the cache and saves
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddFriend(string from, string? to)
        {
            FikaPlayerRelations fromRelations = playerRelationsService.GetStoredValue(from);
            if (!fromRelations.Friends.Contains(from))
            {
                fromRelations.Friends.Add(from);
            }

            FikaPlayerRelations toRelations = playerRelationsService.GetStoredValue(to);
            if (!toRelations.Friends.Contains(from))
            {
                toRelations.Friends.Add(from);
            }

            playerRelationsService.SaveProfileRelations();
        }

        public enum ERemoveFriendReason
        {
            Accept,
            Cancel,
            Decline
        }
    }
}
