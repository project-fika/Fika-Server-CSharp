using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class InsuranceService(SaveServer saveServer, ItemHelper itemHelper, ISptLogger<InsuranceService> logger)
    {
        public string GetMatchId(string sessionID)
        {
            //Todo: Stub for now, implement method.
            return string.Empty;
        }

        public void AddPlayerToMatchId(string matchId, string sessionID)
        {
            //Todo: Stub for now, implement method.
        }

        public void OnEndLocalRaidRequest(string sessionID, string matchId, EndLocalRaidRequestData endLocalRaidRequest)
        {
            //Todo: Stub for now, implement method.
        }

        public void OnMatchEnd(string matchId)
        {
            //Todo: Stub for now, implement method.
        }

        private void RemoveItemsFromInsurance(string sessionID, string[] ids)
        {
            //Todo: Stub for now, implement method.
        }
    }
}
