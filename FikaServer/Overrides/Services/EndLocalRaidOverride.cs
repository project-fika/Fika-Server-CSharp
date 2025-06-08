using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using InsuranceService = FikaServer.Services.InsuranceService;

namespace FikaServer.Overrides.Services
{
    [Injectable]
    public class EndLocalRaidOverride(ISptLogger<LocationLifecycleService> logger, RewardHelper rewardHelper, 
        ConfigServer configServer, TimeUtil timeUtil, DatabaseService databaseService,
        ProfileHelper profileHelper, HashUtil hashUtil, ProfileActivityService profileActivityService,
        BotGenerationCacheService botGenerationCacheService, BotNameService botNameService, ICloner cloner,
        RaidTimeAdjustmentService raidTimeAdjustmentService, LocationLootGenerator locationLootGenerator
        , LocalisationService localisationService, BotLootCacheService botLootCacheService, LootGenerator lootGenerator,
        MailSendService mailSendService, TraderHelper traderHelper, RandomUtil randomUtil, InRaidHelper inRaidHelper,
        PlayerScavGenerator playerScavGenerator, SaveServer saveServer, HealthHelper healthHelper, PmcChatResponseService pmcChatResponseService,
        PmcWaveGenerator pmcWaveGenerator, QuestHelper questHelper, SPTarkov.Server.Core.Services.InsuranceService sptInsuranceService,
        MatchBotDetailsCacheService matchBotDetailsCacheService, MatchService matchService, InsuranceService fikaInsuranceService)
        : LocationLifecycleService(logger, rewardHelper, configServer, timeUtil, databaseService, profileHelper, hashUtil,
            profileActivityService, botGenerationCacheService, botNameService, cloner, raidTimeAdjustmentService, locationLootGenerator,
            localisationService, botLootCacheService, lootGenerator, mailSendService, traderHelper, randomUtil, inRaidHelper, playerScavGenerator,
            saveServer, healthHelper, pmcChatResponseService, pmcWaveGenerator, questHelper, sptInsuranceService, matchBotDetailsCacheService)
    {

        public override void EndLocalRaid(string sessionId, EndLocalRaidRequestData request)
        {
            // Get match id from player session id
            var matchId = matchService.GetMatchIdByPlayer(sessionId);

            // Find player that exited the raid
            var player = matchService.GetPlayerInMatch(matchId, sessionId);

            if (player is not null)
            {
                fikaInsuranceService.OnEndLocalRaidRequest(sessionId, fikaInsuranceService.GetMatchId(sessionId), request);

                // If the player is not a spectator, continue running EndLocalRaid
                if (!player.IsSpectator)
                {
                    base.EndLocalRaid(sessionId, request);
                }
            }
        }
    }
}
