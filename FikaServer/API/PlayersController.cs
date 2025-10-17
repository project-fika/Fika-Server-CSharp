using FikaServer.Models;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Presence;
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaShared;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using static FikaShared.Enums;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/players")]
[RequireApiKey]
public class PlayersController(FikaProfileService profileService, SaveServer saveServer, PresenceService presenceService,
    ILogger<PlayersController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult HandleRequest()
    {
        var players = profileService.GetAllProfiles();
        var profiles = new List<SptProfile>(players.Count);
        foreach (var profileId in players.Values)
        {
            var profile = saveServer.GetProfile(profileId);
            if (profile != null)
            {
                profiles.Add(profile);
            }
        }

        var presences = presenceService.AllPlayersPresence;
        var onlinePlayers = new List<OnlinePlayer>(presences.Count);
        profiles = [.. profiles.Where(x => x.HasProfileData() && presences.Any(p => p.Nickname == x.CharacterData.PmcData.Info.Nickname))];
        foreach (var profile in profiles)
        {
            var profileId = profile.ProfileInfo.ProfileId.Value;
            var presence = presenceService.GetPlayerPresence(profileId);

            EFikaLocation location;
            if (presence != null)
            {
                if (presence.RaidInformation != null)
                {
                    logger.LogInformation("Presence was not null, {Location}", presence.RaidInformation.Location); 
                }
                location = ToFikaLocation(presence);
                logger.LogInformation("Location was {Location}", location);
            }
            else
            {
                logger.LogWarning("Present was null when trying to fetch info for {Nickname}", profile.CharacterData?.PmcData?.Info?.Nickname ?? profileId);
                location = EFikaLocation.None;
            }

            onlinePlayers.Add(new()
            {
                ProfileId = profileId,
                Nickname = profile.CharacterData?.PmcData?.Info?.Nickname ?? "Unknown",
                Level = profile.CharacterData?.PmcData?.Info?.Level ?? 1,
                Location = location
            });
        }

        GetOnlinePlayersResponse playersResponse = new()
        {
            Players = onlinePlayers
        };

        return Ok(playersResponse);
    }

    private EFikaLocation ToFikaLocation(FikaPlayerPresence presence)
    {
        if (presence.RaidInformation != null)
        {
            switch (presence.RaidInformation.Location)
            {
                case "bigmap":
                    return EFikaLocation.Customs;
                case "factory4_day":
                case "factory4_night":
                    return EFikaLocation.Factory;
                case "interchange":
                    return EFikaLocation.Interchange;
                case "laboratory":
                    return EFikaLocation.Laboratory;
                case "labyrinth":
                    return EFikaLocation.Labyrinth;
                case "lighthouse":
                    return EFikaLocation.Lighthouse;
                case "rezervbase":
                    return EFikaLocation.Reserve;
                case "sandbox":
                case "sandbox_high":
                    return EFikaLocation.GroundZero;
                case "shoreline":
                    return EFikaLocation.Shoreline;
                case "tarkovstreets":
                    return EFikaLocation.Streets;
                case "woods":
                    return EFikaLocation.Woods;
                default:
                    logger.LogWarning("Location was incorrect when getting presense for {Nickname}", presence.Nickname);
                    return EFikaLocation.None;
            }
        }

        if (presence.Activity is EFikaPlayerPresences.IN_HIDEOUT)
        {
            return EFikaLocation.Hideout;
        }

        return EFikaLocation.None;
    }
}
