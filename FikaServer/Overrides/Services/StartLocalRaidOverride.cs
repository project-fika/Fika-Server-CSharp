using FikaServer.Services;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace FikaServer.Overrides.Services
{
    public class StartLocalRaidOverride : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(LocationLifecycleService).GetMethod(nameof(LocationLifecycleService.StartLocalRaid));
        }

        [PatchPrefix]
        public static bool Prefix(string sessionId, StartLocalRaidRequestData request, ref StartLocalRaidResponseData __result)
        {
            LocationBase locationLoot;
            MatchService matchService = ServiceLocator.ServiceProvider.GetService<MatchService>()!;
            LocationLifecycleService locationLifeCycleService = ServiceLocator.ServiceProvider.GetService<LocationLifecycleService>()!;

            var matchId = matchService!.GetMatchIdByProfile(sessionId);

            if (string.IsNullOrEmpty(matchId))
            {
                // player isn't in a Fika match, generate new loot
                locationLoot = locationLifeCycleService!.GenerateLocationAndLoot(sessionId, request.Location);
            }
            else
            {
                // player is in a Fika match, use match location loot and regen if transit
                var match = matchService.GetMatch(matchId);

                if (matchId == sessionId)
                {
                    match.Raids++;
                    if (match.Raids > 1)
                    {
                        match.LocationData = locationLifeCycleService!.GenerateLocationAndLoot(sessionId, request.Location);
                    }
                }

                locationLoot = match.LocationData;
            }

            DatabaseService databaseService = ServiceLocator.ServiceProvider.GetService<DatabaseService>()!;
            ProfileHelper profileHelper = ServiceLocator.ServiceProvider.GetService<ProfileHelper>()!;
            TimeUtil timeUtil = ServiceLocator.ServiceProvider.GetService<TimeUtil>()!;

            var playerProfile = profileHelper.GetPmcProfile(sessionId);

            StartLocalRaidResponseData result = new()
            {
                ServerId = $"{request.Location}.{request.PlayerSide}.{timeUtil.GetTimeStamp()}",
                ServerSettings = databaseService.GetLocationServices(),
                Profile = new ProfileInsuredItems
                {
                    InsuredItems = playerProfile.InsuredItems
                },
                LocationLoot = locationLoot,
                Transition = new Transition
                {
                    TransitionType = SPTarkov.Server.Core.Models.Enums.TransitionType.COMMON,
                    TransitionRaidId = "66f5750951530ca5ae09876d",
                    TransitionCount = 0,
                    VisitedLocations = []
                }
            };

            // Only has value when transitioning into map from previous one
            if (request.Transition != null)
            {
                result.Transition = request.Transition;
            }

            // Get data stored at end of previous raid (if any)
            LocationTransit? transitionData = ServiceLocator.ServiceProvider.GetService<ProfileActivityService>()!.GetProfileActivityRaidData(sessionId).LocationTransit;

            if (transitionData != null)
            {
                result.Transition.TransitionRaidId = transitionData.TransitionRaidId;
                result.Transition.TransitionCount += 1;

                if (result.Transition.VisitedLocations == null)
                {
                    result.Transition.VisitedLocations = [transitionData.SptLastVisitedLocation];
                }
                else
                {
                    result.Transition.VisitedLocations.Add(transitionData.SptLastVisitedLocation);
                }

                // Complete, clean up
                ServiceLocator.ServiceProvider.GetService<ProfileActivityService>()!.GetProfileActivityRaidData(sessionId).LocationTransit = null;
            }

            if (string.IsNullOrEmpty(matchId) || sessionId == matchId)
            {
                // Apply changes from pmcConfig to bot hostility values
                typeof(LocationLifecycleService).GetMethod("AdjustBotHostilitySettings")?.Invoke(locationLifeCycleService, [result.LocationLoot]);
                typeof(LocationLifecycleService).GetMethod("AdjustExtracts")?.Invoke(locationLifeCycleService, [request.PlayerSide, request.Location, result.LocationLoot]);

                // Clear bot cache ready for a fresh raid
                ServiceLocator.ServiceProvider.GetService<BotGenerationCacheService>()!.ClearStoredBots();
                ServiceLocator.ServiceProvider.GetService<BotNameService>()!.ClearNameCache();
            }

            __result = result;

            return false;
        }
    }
}
