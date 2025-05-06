using FikaServer.Models.Fika;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;

namespace FikaServer.Helpers
{
    [Injectable]
    public class PlayerRelationsHelper(PlayerRelationsService playerRelationsService)
    {
        public List<string> GetFriendsList(string profileId)
        {
            return playerRelationsService.GetStoredValue(profileId).Friends;
        }

        public void RemoveFriend(string sessionId, string friend)
        {
            // TODO: Add
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
