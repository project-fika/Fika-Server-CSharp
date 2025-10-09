using FikaServer.Models;
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
public class PlayersController(FikaProfileService profileService, SaveServer saveServer, PresenceService presenceService) : ControllerBase
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
                location = presence.ToFikaLocation();
            }
            else
            {
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
}
