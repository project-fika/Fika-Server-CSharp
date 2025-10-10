using FikaServer.Models;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/statistics")]
[RequireApiKey]
public class GetStatisticsController(SaveServer saveServer) : ControllerBase
{
    public IActionResult HandleRequest()
    {
        var players = saveServer.GetProfiles().Values
            .Where(x => x.HasProfileData());


        var statisticsPlayers = new List<StatisticsPlayer>();
        foreach (var player in players)
        {
            if (player.IsHeadlessProfile())
            {
                continue; // ignore headless
            }

            var statPlayer = new StatisticsPlayer
            {
                Nickname = player.CharacterData.PmcData.Info.Nickname,
                Kills = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("Kills"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                Deaths = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("Deaths"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                AmmoUsed = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("AmmoUsed"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                BodyDamage = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("CauseBodyDamage"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                ArmorDamage = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("CauseArmorDamage"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                Headshots = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("HeadShots"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
                BossKills = (player.CharacterData?.PmcData?.Stats?.Eft?.OverallCounters?.Items?
                    .Where(x => x.Key?.Count == 1 && x.Key.Contains("KilledBoss"))
                    .Select(x => x.Value)
                    .FirstOrDefault() ?? 0.0),
            };

            statisticsPlayers.Add(statPlayer);
        }

        var response = new GetStatisticsResponse
        {
            Players = statisticsPlayers
        };

        return Ok(response);
    }
}
