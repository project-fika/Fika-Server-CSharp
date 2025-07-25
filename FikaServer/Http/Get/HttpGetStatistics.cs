
using FikaServer.Services;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetStatistics(ConfigService configService, SaveServer saveServer,
        PresenceService presenceService, HttpResponseUtil httpResponseUtil) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/statistics";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            var players = saveServer.GetProfiles().Values
                .Where(x => x.HasProfileData());


            var statisticsPlayers = new List<StatisticsPlayer>();
            foreach (var player in players)
            {
                var statPlayer = new StatisticsPlayer
                {
                    Nickname = player.CharacterData.PmcData.Info.Nickname,
                    Kills = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("Kills"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    Deaths = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("Deaths"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    AmmoUsed = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("AmmoUsed"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    BodyDamage = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("CauseBodyDamage"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    ArmorDamage = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("CauseArmorDamage"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    Headshots = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("HeadShots"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                    BossKills = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                        .Where(x => x.Key?.Count() == 1 && x.Key.Contains("KilledBoss"))
                        .Select(x => x.Value)
                        .FirstOrDefault() ?? 0.0),
                };

                statisticsPlayers.Add(statPlayer);
            }

            var response = new GetStatisticsResponse
            {
                Players = statisticsPlayers
            };

            resp.StatusCode = 200;
            resp.ContentType = ContentTypes.Json;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(response)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
