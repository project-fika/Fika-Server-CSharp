using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace FikaServer.Overrides.Services
{
    [Injectable]
    public class StartLocalRaidOverride(ISptLogger<LocationLifecycleService> logger, RewardHelper rewardHelper,
        ConfigServer configServer, TimeUtil timeUtil, DatabaseService databaseService,
        ProfileHelper profileHelper, HashUtil hashUtil, ProfileActivityService profileActivityService,
        BotGenerationCacheService botGenerationCacheService, BotNameService botNameService, ICloner cloner,
        RaidTimeAdjustmentService raidTimeAdjustmentService, LocationLootGenerator locationLootGenerator
        , LocalisationService localisationService, BotLootCacheService botLootCacheService, LootGenerator lootGenerator,
        MailSendService mailSendService, TraderHelper traderHelper, RandomUtil randomUtil, InRaidHelper inRaidHelper,
        PlayerScavGenerator playerScavGenerator, SaveServer saveServer, HealthHelper healthHelper, PmcChatResponseService pmcChatResponseService,
        PmcWaveGenerator pmcWaveGenerator, QuestHelper questHelper, SPTarkov.Server.Core.Services.InsuranceService sptInsuranceService,
        MatchBotDetailsCacheService matchBotDetailsCacheService, MatchService matchService)
        : LocationLifecycleService(logger, rewardHelper, configServer, timeUtil, databaseService, profileHelper, hashUtil,
            profileActivityService, botGenerationCacheService, botNameService, cloner, raidTimeAdjustmentService, locationLootGenerator,
            localisationService, botLootCacheService, lootGenerator, mailSendService, traderHelper, randomUtil, inRaidHelper, playerScavGenerator,
            saveServer, healthHelper, pmcChatResponseService, pmcWaveGenerator, questHelper, sptInsuranceService, matchBotDetailsCacheService)
    {
        public override StartLocalRaidResponseData StartLocalRaid(string sessionId, StartLocalRaidRequestData request)
        {
            LocationBase locationLoot;

            var matchId = matchService!.GetMatchIdByProfile(sessionId);

            if (string.IsNullOrEmpty(matchId))
            {
                // player isn't in a Fika match, generate new loot
                locationLoot = GenerateLocationAndLoot(sessionId, request.Location);
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
                        match.LocationData = GenerateLocationAndLoot(sessionId, request.Location);
                    }
                }

                locationLoot = match.LocationData;
            }
             
            var playerProfile = profileHelper.GetPmcProfile(sessionId);

            StartLocalRaidResponseData responseData = new()
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
                responseData.Transition = request.Transition;
            }

            // Get data stored at end of previous raid (if any)
            LocationTransit? transitionData = profileActivityService.GetProfileActivityRaidData(sessionId).LocationTransit;

            if (transitionData != null)
            {
                responseData.Transition.TransitionRaidId = transitionData.TransitionRaidId;
                responseData.Transition.TransitionCount += 1;

                if (responseData.Transition.VisitedLocations == null)
                {
                    responseData.Transition.VisitedLocations = [transitionData.SptLastVisitedLocation];
                }
                else
                {
                    responseData.Transition.VisitedLocations.Add(transitionData.SptLastVisitedLocation);
                }

                // Complete, clean up
                profileActivityService.GetProfileActivityRaidData(sessionId).LocationTransit = null;
            }

            if (string.IsNullOrEmpty(matchId) || sessionId == matchId)
            {
                // Apply changes from pmcConfig to bot hostility values
                AdjustBotHostilitySettings(responseData.LocationLoot);
                AdjustExtracts(request.PlayerSide, request.Location, responseData.LocationLoot);

                // Clear bot cache ready for a fresh raid
                botGenerationCacheService.ClearStoredBots();
                botNameService.ClearNameCache();
            }

            return responseData;
        }
    }
}
