using FikaServer.Models.Fika.Dialog;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Text.Json;

namespace FikaServer.Services.Cache
{
    [Injectable(InjectionType.Singleton)]
    public class FriendRequestsService(
        ProfileHelper profileHelper,
        ConfigService FikaConfig,
        SaveServer saveServer,
        ISptLogger<PlayerRelationsService> logger)
    {
        public List<FriendRequestListResponse> AllFriendRequests
        {
            get
            {
                return [.. _friendRequests];
            }
        }

        private readonly string _friendRequestsFullPath = Path.Join(FikaConfig.GetModPath(), "database");
        private List<FriendRequestListResponse> _friendRequests = [];

        private readonly Lock _listLock = new();

        public void OnPreLoad()
        {
            if (!Directory.Exists(_friendRequestsFullPath))
            {
                Directory.CreateDirectory(_friendRequestsFullPath);
            }

            string file = $"{_friendRequestsFullPath}/friendRequests.json";
            if (!File.Exists(file))
            {
                SaveFriendRequests();
            }
            else
            {
                string data = File.ReadAllText(file);
                _friendRequests = _friendRequests = JsonSerializer.Deserialize<List<FriendRequestListResponse>>(data, ConfigService.serializerOptions);
            }
        }

        public void OnPostLoad()
        {
            logger.Debug($"Loaded {_friendRequests.Count} friend requests");
        }

        public void AddFriendRequest(FriendRequestListResponse friendRequest)
        {
            lock (_listLock)
            {
                _friendRequests.Add(friendRequest);
                SaveFriendRequests();
            }
        }

        public void DeleteFriendRequest(FriendRequestListResponse friendRequest)
        {
            lock (_listLock)
            {
                if (!_friendRequests.Remove(friendRequest))
                {
                    logger.Error($"Unable to remove friend request {friendRequest.Id}");
                    return;
                }

                SaveFriendRequests();
            }
        }

        /// <summary>
        /// Checks if there is a friend request between to ProfileIDs
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool HasFriendRequest(string from, string to)
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
        public bool HasFriendRequest(string from, string to, out FriendRequestListResponse response)
        {
            lock (_listLock)
            {
                response = _friendRequests.Single(x => x.From == from && x.To == to);
                return response != null;
            }
        }

        public List<FriendRequestListResponse> GetReceivedFriendRequests(string profileId)
        {
            lock (_listLock)
            {
                return [.. _friendRequests
                    .Where(x => x.To == profileId)];
            }
        }

        public List<FriendRequestListResponse> GetSentFriendRequests(string profileId)
        {
            lock (_listLock)
            {
                return [.. _friendRequests
                    .Where(x => x.From == profileId)];
            }
        }

        private void SaveFriendRequests()
        {
            File.WriteAllText($"{_friendRequestsFullPath}/friendRequests.json",
                JsonSerializer.Serialize(_friendRequests,
                ConfigService.serializerOptions));
        }
    }
}
