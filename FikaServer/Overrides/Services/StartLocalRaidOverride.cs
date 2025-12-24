using System.Reflection;
using FikaServer.Services;
using FikaServer.Services.Headless;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Overrides.Services;

public class StartLocalRaidOverride : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocationLifecycleService)
            .GetMethod(nameof(LocationLifecycleService.StartLocalRaid))!;
    }

    [PatchPrefix]
    public static bool Prefix(MongoId sessionId, StartLocalRaidRequestData request, ref StartLocalRaidResponseData __result)
    {
        LocationBase location;
        var matchService = ServiceLocator.ServiceProvider.GetService<MatchService>()
            ?? throw new NullReferenceException("MatchService is null!");
        var locationLifeCycleService = ServiceLocator.ServiceProvider.GetService<LocationLifecycleService>()
            ?? throw new NullReferenceException("LocationLifecycleService is null!");

        var matchId = matchService!.GetMatchIdByProfile(sessionId);

        if (string.IsNullOrEmpty(matchId))
        {
            // player isn't in a Fika match, generate new loot
            location = locationLifeCycleService.GenerateLocationAndLoot(sessionId, request!.Location!, request!.ShouldSkipLootGeneration ?? true);
        }
        else
        {
            // player is in a Fika match, use match location loot and regen if transit
            var match = matchService.GetMatch(matchId);

            if (matchId == sessionId)
            {
                // force another level set due to transits
                if (match!.IsHeadless)
                {
                    var headlessService = ServiceLocator.ServiceProvider.GetService<HeadlessService>()
                        ?? throw new NullReferenceException("HeadlessService is null");

                    headlessService.SetHeadlessLevel(sessionId);
                }

                match!.Raids++;
                if (match.Raids > 1)
                {
                    match.LocationData = locationLifeCycleService.GenerateLocationAndLoot(sessionId, request!.Location!, request!.ShouldSkipLootGeneration ?? true);
                }
            }

            location = match!.LocationData;
        }

        var databaseService = ServiceLocator.ServiceProvider.GetService<DatabaseService>()
            ?? throw new NullReferenceException("DatabaseService is null!");
        var profileHelper = ServiceLocator.ServiceProvider.GetService<ProfileHelper>()
            ?? throw new NullReferenceException("ProfileHelper is null!");
        var timeUtil = ServiceLocator.ServiceProvider.GetService<TimeUtil>()
            ?? throw new NullReferenceException("TimeUtil is null!");

        var playerProfile = profileHelper.GetFullProfile(sessionId);

        var isSide = (bool)typeof(LocationLifecycleService)
            .GetMethod("IsSide", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(locationLifeCycleService, [request.PlayerSide!, "pmc"])!;
        typeof(LocationLifecycleService)
            .GetMethod("ResetSkillPointsEarnedDuringRaid", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(locationLifeCycleService, [isSide
                ? playerProfile!.CharacterData!.PmcData!.Skills!.Common
                : playerProfile!.CharacterData!.ScavData!.Skills!.Common]);

        var transitionType = TransitionType.NONE;

        if (request.TransitionType is TransitionType flags)
        {
            if (flags.HasFlag(TransitionType.COMMON))
            {
                transitionType = TransitionType.COMMON;
            }

            if (flags.HasFlag(TransitionType.EVENT))
            {
                transitionType = TransitionType.EVENT;
            }
        }

        var isRundansActive = databaseService.GetGlobals().Configuration.RunddansSettings.Active;

        if (transitionType == TransitionType.EVENT)
        {
            var configServer = ServiceLocator.ServiceProvider.GetService<ConfigServer>()
                ?? throw new NullReferenceException("ConfigServer is null!");
            var seasonalEventConfig = configServer.GetConfig<SeasonalEventConfig>();
            // Handle Runddans / Khorovod event
            if (isRundansActive && location.Transits is not null)
            {
                // Get whitelist for maps transits, event should have 1 only
                var matchingTransitWhitelist = seasonalEventConfig.KhorovodEventTransitWhitelist.GetValueOrDefault(
                    location.Id.ToLowerInvariant(),
                    []
                );

                foreach (var transits in location.Transits)
                {
                    if (transits.Id is null)
                    {
                        continue;
                    }

                    // ActivateAfterSeconds sets the timer on the generator, events is needed because it is checked again in the client
                    // To enable certain stuff for the Khorovod event
                    if (matchingTransitWhitelist.Contains(transits.Id.Value))
                    {
                        transits.ActivateAfterSeconds = 300;
                        transits.Events = true;
                    }
                    else
                    {
                        // Disable the other transits in this event, people are only allowed to transit to certain points
                        transits.IsActive = false;
                    }
                }
            }
        }

        StartLocalRaidResponseData result = new()
        {
            ServerId = $"{request.Location}.{request.PlayerSide}.{timeUtil.GetTimeStamp()}",
            ServerSettings = databaseService.GetLocationServices(),
            Profile = new ProfileInsuredItems
            {
                InsuredItems = playerProfile!.CharacterData!.PmcData!.InsuredItems
            },
            LocationLoot = location,
            TransitionType = transitionType,
            Transition = new Transition
            {
                TransitionType = transitionType,
                TransitionRaidId = new MongoId(),
                TransitionCount = 0,
                VisitedLocations = []
            },
            ExcludedBosses = []
        };

        // Only has value when transitioning into map from previous one
        if (request.Transition != null)
        {
            result.Transition = request.Transition;
        }

        // Get data stored at end of previous raid (if any)
        var transitionData = ServiceLocator.ServiceProvider.GetService<ProfileActivityService>()!
            .GetProfileActivityRaidData(sessionId).LocationTransit;

        if (transitionData != null)
        {
            result.Transition.TransitionType = transitionType;
            result.Transition.TransitionRaidId = transitionData.TransitionRaidId;
            result.Transition.TransitionCount += 1;

            if (result.Transition.VisitedLocations == null)
            {
                result.Transition.VisitedLocations = [transitionData!.SptLastVisitedLocation!];
            }
            else
            {
                result.Transition.VisitedLocations.Add(transitionData!.SptLastVisitedLocation!);
            }

            // Complete, clean up
            ServiceLocator.ServiceProvider.GetService<ProfileActivityService>()!
                .GetProfileActivityRaidData(sessionId).LocationTransit = null;
        }

        if (string.IsNullOrEmpty(matchId) || sessionId == matchId.Value)
        {
            // Apply changes from pmcConfig to bot hostility values
            typeof(LocationLifecycleService)
                .GetMethod("AdjustBotHostilitySettings", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(locationLifeCycleService, [result.LocationLoot]);
            typeof(LocationLifecycleService)
                .GetMethod("AdjustExtracts", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(locationLifeCycleService, [request.PlayerSide, request.Location, result.LocationLoot]);

            // Clear bot cache ready for bot generation call that occurs after this
            ServiceLocator.ServiceProvider.GetService<BotNameService>()!
                .ClearNameCache();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        }

        // Handle Player Inventory Wiping checks for alt-f4 prevention
        typeof(LocationLifecycleService)
            .GetMethod("HandlePreRaidInventoryChecks", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(locationLifeCycleService, [request.PlayerSide, playerProfile.CharacterData.PmcData, sessionId]);

        __result = result;

        return false;
    }
}
