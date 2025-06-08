using FikaServer.Services;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Services;
using System.Reflection;
using InsuranceService = FikaServer.Services.InsuranceService;

namespace FikaServer.Overrides.Services
{
    public class EndLocalRaidOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocationLifecycleService).GetMethod(nameof(LocationLifecycleService.EndLocalRaid));
        }

        [PatchPrefix]
        public static bool Prefix(string sessionId, EndLocalRaidRequestData request)
        {
            MatchService matchService = ServiceLocator.ServiceProvider.GetService<MatchService>();
            InsuranceService insuranceService = ServiceLocator.ServiceProvider.GetService<InsuranceService>();

            // Get match id from player session id
            var matchId = matchService.GetMatchIdByPlayer(sessionId);

            // Find player that exited the raid
            var player = matchService.GetPlayerInMatch(matchId, sessionId);

            if (player is not null)
            {
                insuranceService.OnEndLocalRaidRequest(sessionId, insuranceService.GetMatchId(sessionId), request);

                // If the player is not a spectator, continue running EndLocalRaid
                if (!player.IsSpectator)
                {
                    return true;
                }
            }

            // Stop running the method if the player is a spectator
            return false;
        }
    }
}
