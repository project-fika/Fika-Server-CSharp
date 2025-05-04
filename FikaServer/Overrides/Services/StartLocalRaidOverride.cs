using FikaServer.Services;
using HarmonyLib;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Overrides.Services
{
    [HarmonyPatch(typeof(LocationLifecycleService))]
    [HarmonyPatch(nameof(LocationLifecycleService.StartLocalRaid))]
    public class StartLocalRaidOverride
    {
        public static bool Prefix(string sessionId, StartLocalRaidRequestData request, ref StartLocalRaidResponseData __result)
        {
            ServiceProvider? sp = ApplicationContext.GetInstance()?.GetLatestValue(ContextVariableType.SERVICE_PROVIDER)?.GetValue<ServiceProvider>();

            if (sp != null)
            {
                LocationBase locationLoot;
                MatchService matchService = sp.GetService<MatchService>()!;
                LocationLifecycleService locationLifeCycleService = sp.GetService<LocationLifecycleService>()!;

                var matchId = matchService!.GetMatchIdByProfile(sessionId);

                if (string.IsNullOrEmpty(matchId))
                {
                    // player isn't in a Fika match, generate new loot
                    locationLoot = locationLifeCycleService!.GenerateLocationAndLoot(request.Location);
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
                            match.LocationData = locationLifeCycleService!.GenerateLocationAndLoot(request.Location);
                        }
                    }

                    locationLoot = match.LocationData;
                }

                DatabaseService databaseService = sp.GetService<DatabaseService>()!;
                ProfileHelper profileHelper = sp.GetService<ProfileHelper>()!;
                TimeUtil timeUtil = sp.GetService<TimeUtil>()!;

                var playerProfile = profileHelper.GetPmcProfile(sessionId);

                StartLocalRaidResponseData result = new StartLocalRaidResponseData
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
                LocationTransit? transitionData = ApplicationContext.GetInstance()?.GetLatestValue(ContextVariableType.TRANSIT_INFO)?.GetValue<LocationTransit>();

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
                    ApplicationContext.GetInstance()?.ClearValues(ContextVariableType.TRANSIT_INFO);
                }

                if (string.IsNullOrEmpty(matchId) || sessionId == matchId)
                {
                    // Apply changes from pmcConfig to bot hostility values
                    typeof(LocationLifecycleService).GetMethod("AdjustBotHostilitySettings")?.Invoke(locationLifeCycleService, [result.LocationLoot]);
                    typeof(LocationLifecycleService).GetMethod("AdjustExtracts")?.Invoke(locationLifeCycleService, [request.PlayerSide, request.Location, result.LocationLoot]);

                    // Clear bot cache ready for a fresh raid
                    sp.GetService<BotGenerationCacheService>()!.ClearStoredBots();
                    sp.GetService<BotNameService>()!.ClearNameCache();
                }

                __result = result;
            }

            return false;
        }
    }
}
