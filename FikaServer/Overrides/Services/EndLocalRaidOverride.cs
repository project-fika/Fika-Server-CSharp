using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Services.InRaid;
using System.Reflection;
using InsuranceService = FikaServer.Services.InsuranceService;

namespace FikaServer.Overrides.Services;

[Injectable]
public sealed class EndLocalRaidOverride : AbstractPatch
{
    private static MatchService _matchService = default!;
    private static InsuranceService _insuranceService = default!;

    public EndLocalRaidOverride(MatchService matchService, InsuranceService insuranceService)
    {
        _matchService = matchService;
        _insuranceService = insuranceService;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocationLifecycleService)
            .GetMethod(nameof(LocationLifecycleService.EndLocalRaidAsync))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, EndLocalRaidRequestData request)
    {
        // Get match id from player session id
        var matchId = _matchService.GetMatchIdByPlayer(sessionId);
        if (matchId == null)
        {
            // Could not find matchId, run original
            return true;
        }

        // Find player that exited the raid
        var player = _matchService.GetPlayerInMatch(matchId.Value, sessionId);

        if (player != null)
        {
            _insuranceService.OnEndLocalRaidRequest(sessionId, _insuranceService.GetMatchId(sessionId), request);

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
