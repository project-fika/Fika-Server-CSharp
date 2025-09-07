using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Dialog;
using FikaServer.Models.Fika.WebSocket;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;

namespace FikaServer.Helpers;

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
    public List<string> GetFriendsList(MongoId profileId)
    {
        return playerRelationsService.GetStoredValue(profileId).Friends;
    }

    /// <summary>
    /// Removes a friend relation
    /// </summary>
    /// <param name="fromProfileId">Requesting profileId</param>
    /// <param name="toProfileId">Target profileId</param>
    /// <exception cref="NotImplementedException"></exception>
    public void RemoveFriend(MongoId fromProfileId, MongoId toProfileId)
    {
        FikaPlayerRelations fromRelations = playerRelationsService.GetStoredValue(fromProfileId);
        if (fromRelations == null)
        {
            logger.Debug($"Could not find relations for {fromProfileId}");
            return;
        }

        FikaPlayerRelations toRelations = playerRelationsService.GetStoredValue(toProfileId);
        if (toRelations == null)
        {
            logger.Debug($"Could not find relations for target {toProfileId}");
            return;
        }

        if (!fromRelations.Friends.Remove(toProfileId))
        {
            logger.Warning($"{fromProfileId} tried to remove {toProfileId} from their friend list unsuccessfully");
        }

        if (!toRelations.Friends.Remove(fromProfileId))
        {
            logger.Warning($"{toProfileId} tried to remove {fromProfileId} from their friend list unsuccessfully");
        }

        SptProfile profile = saveServer.GetProfile(fromProfileId);
        if (profile != null && profile.ProfileInfo != null && profile.CharacterData?.PmcData?.Info != null)
        {
            webSocketHandler.SendMessage(toProfileId, new WsFriendListRemove()
            {
                EventIdentifier = new(),
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
    public List<string> GetIgnoreList(MongoId profileId)
    {
        return playerRelationsService.GetStoredValue(profileId).Ignore;
    }

    /// <summary>
    /// Returns a list of players ignoring this player
    /// </summary>
    /// <param name="profileId">The player to check for</param>
    /// <returns>List of players ignoring the player</returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<string> GetInIgnoreList(MongoId profileId)
    {
        return [.. playerRelationsService.Keys
            .Where(x => playerRelationsService.GetStoredValue(x).Ignore.Contains(profileId))];
    }

    public bool RemoveFriendRequest(MongoId from, MongoId to, ERemoveFriendReason reason)
    {
        if (!friendRequestsService.HasFriendRequest(from, to, out FriendRequestListResponse? response))
        {
            logger.Error($"{from} tried to remove a friend request from {to} but it doesn't exist. Reason: {reason}");
            return false;
        }

        friendRequestsService.DeleteFriendRequest(response)
            .GetAwaiter().GetResult();

        switch (reason)
        {
            case ERemoveFriendReason.Accept:
                {
                    SptProfile profile = saveServer.GetProfile(to);
                    webSocketHandler.SendMessage(from, new WsFriendListRemove()
                    {
                        EventIdentifier = new(),
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
                        EventIdentifier = new(),
                        EventType = NotificationEventType.friendListRequestCancel,
                        Profile = profile.ToFriendData()
                    });
                }
                break;
            case ERemoveFriendReason.Decline:
                {
                    SptProfile profile = saveServer.GetProfile(to);
                    webSocketHandler.SendMessage(from, new WsFriendListRemove()
                    {
                        EventIdentifier = new(),
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
    public void AddFriend(MongoId from, MongoId to)
    {
        FikaPlayerRelations fromRelations = playerRelationsService.GetStoredValue(from);
        if (!fromRelations.Friends.Contains(to))
        {
            fromRelations.Friends.Add(to);
        }

        FikaPlayerRelations toRelations = playerRelationsService.GetStoredValue(to);
        if (!toRelations.Friends.Contains(from))
        {
            toRelations.Friends.Add(from);
        }

        playerRelationsService.SaveProfileRelations();
    }

    public void AddToIgnoreList(MongoId from, MongoId to)
    {
        FikaPlayerRelations fromRelations = playerRelationsService.GetStoredValue(from);
        if (fromRelations.Ignore.Contains(to))
        {
            logger.Warning($"{from} already has {to} in their ignore list");
            return;
        }

        fromRelations.Ignore.Add(to);
        playerRelationsService.SaveProfileRelations();

        SptProfile profile = saveServer.GetProfile(from);
        ArgumentNullException.ThrowIfNull(profile);

        webSocketHandler.SendMessage(to, new WsIgnoreListAdd()
        {
            EventIdentifier = new(),
            EventType = NotificationEventType.youAreAddToIgnoreList,
            Id = from,
            Profile = profile.ToFriendData()
        });
    }

    public void RemoveFromIgnoreList(MongoId from, MongoId to)
    {
        FikaPlayerRelations fromRelations = playerRelationsService.GetStoredValue(from);
        if (!fromRelations.Ignore.Remove(to))
        {
            logger.Warning($"{from} tried to remove {to} from their ignore list but it was unsuccesful");
            return;
        }

        playerRelationsService.SaveProfileRelations();

        SptProfile profile = saveServer.GetProfile(from);
        ArgumentNullException.ThrowIfNull(profile);

        webSocketHandler.SendMessage(to, new WsIgnoreListAdd()
        {
            EventIdentifier = new(),
            EventType = NotificationEventType.youAreRemoveFromIgnoreList,
            Id = from,
            Profile = profile.ToFriendData()
        });
    }

    public enum ERemoveFriendReason
    {
        Accept,
        Cancel,
        Decline
    }
}
