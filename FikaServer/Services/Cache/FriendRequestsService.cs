using FikaServer.Models.Fika.Dialog;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Services.Cache
{
    [Injectable(InjectionType.Singleton)]
    public class FriendRequestsService(
        ProfileHelper profileHelper,
        ConfigService FikaConfig,
        SaveServer saveServer,
        JsonUtil jsonUtil,
        ISptLogger<PlayerRelationsService> logger)
    {
        public List<FriendRequestListResponse> AllFriendRequests
        {
            get
            {
                return [.. _friendRequests];
            }
        }

        private readonly string _friendRequestsFullPath = Path.Join(FikaConfig.ModPath, "database");
        private List<FriendRequestListResponse> _friendRequests = [];

        private readonly Lock _listLock = new();

        public async Task OnPreLoad()
        {
            if (!Directory.Exists(_friendRequestsFullPath))
            {
                Directory.CreateDirectory(_friendRequestsFullPath);
            }

            string file = $"{_friendRequestsFullPath}/friendRequests.json";
            if (!File.Exists(file))
            {
                await SaveFriendRequests();
            }
            else
            {
                try
                {
                    string data = File.ReadAllText(file);
                    _friendRequests = await jsonUtil.DeserializeFromFileAsync<List<FriendRequestListResponse>>(file);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to load friend requests: {ex.Message}");
                }
            }
        }

        public void OnPostLoad()
        {
            logger.Debug($"Loaded {_friendRequests.Count} friend requests");
        }

        public async Task AddFriendRequest(FriendRequestListResponse friendRequest)
        {
            lock (_listLock)
            {
                _friendRequests.Add(friendRequest);
            }

            await SaveFriendRequests();
        }

        public async Task DeleteFriendRequest(FriendRequestListResponse friendRequest)
        {
            lock (_listLock)
            {
                if (!_friendRequests.Remove(friendRequest))
                {
                    logger.Error($"Unable to remove friend request {friendRequest.Id}");
                    return;
                }
            }

            await SaveFriendRequests();
        }

        /// <summary>
        /// Checks if there is a friend request between to ProfileIDs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool HasFriendRequest(MongoId from, MongoId to)
        {
            lock (_listLock)
            {
                return _friendRequests
                    .Any(x => x.From == from && x.To == to);
            }
        }

        /// <summary>
        /// Checks if there is a friend request between two ProfileIDs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="response"></param>
        /// <returns>The friend request</returns>
        public bool HasFriendRequest(MongoId from, MongoId to, out FriendRequestListResponse response)
        {
            lock (_listLock)
            {
                response = _friendRequests.Single(x => x.From == from && x.To == to);
                return response != null;
            }
        }

        public List<FriendRequestListResponse> GetReceivedFriendRequests(MongoId profileId)
        {
            lock (_listLock)
            {
                return [.. _friendRequests
                    .Where(x => x.To == profileId)];
            }
        }

        public List<FriendRequestListResponse> GetSentFriendRequests(MongoId profileId)
        {
            lock (_listLock)
            {
                return [.. _friendRequests
                    .Where(x => x.From == profileId)];
            }
        }

        private async Task SaveFriendRequests()
        {
            await File.WriteAllTextAsync($"{_friendRequestsFullPath}/friendRequests.json",
                jsonUtil.Serialize(_friendRequests, true));
        }
    }
}
