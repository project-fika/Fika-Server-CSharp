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
    private static readonly Dictionary<string, EFikaLocation> _locationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bigmap"] = EFikaLocation.Customs,
        ["factory4_day"] = EFikaLocation.Factory,
        ["factory4_night"] = EFikaLocation.Factory,
        ["interchange"] = EFikaLocation.Interchange,
        ["laboratory"] = EFikaLocation.Laboratory,
        ["labyrinth"] = EFikaLocation.Labyrinth,
        ["lighthouse"] = EFikaLocation.Lighthouse,
        ["rezervbase"] = EFikaLocation.Reserve,
        ["sandbox"] = EFikaLocation.GroundZero,
        ["sandbox_high"] = EFikaLocation.GroundZero,
        ["shoreline"] = EFikaLocation.Shoreline,
        ["tarkovstreets"] = EFikaLocation.Streets,
        ["woods"] = EFikaLocation.Woods
    };

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
                location = ToFikaLocation(presence);
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
            if (_locationMap.TryGetValue(presence.RaidInformation.Location, out var eLocation))
            {
                return eLocation;
            }

            logger.LogWarning("Location was incorrect when getting presense for {Nickname}", presence.Nickname);
            return EFikaLocation.None;
        }

        if (presence.Activity is EFikaPlayerPresences.IN_HIDEOUT)
        {
            return EFikaLocation.Hideout;
        }

        return EFikaLocation.None;
    }
}
