using FikaServer.Models;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/profiles")]
[RequireApiKey]
public class ProfilesController(SaveServer saveServer) : ControllerBase
{
    public IActionResult HandleRequest()
    {
        var profiles = saveServer.GetProfiles().Values;
        List<ProfileResponse> profilesResponse = [];
        foreach (SptProfile profile in profiles)
        {
            profilesResponse.Add(new()
            {
                Nickname = profile.CharacterData?.PmcData?.Info?.Nickname ?? "Unknown",
                ProfileId = profile.ProfileInfo?.ProfileId.GetValueOrDefault() ?? "Unknown",
                HasFleaBan = profile.CharacterData?.PmcData?.Info?.Bans?.Any(x => x.BanType is BanType.RagFair) ?? false,
                Level = profile.CharacterData?.PmcData?.Info?.Level ?? 0,
            });
        }

        return Ok(profilesResponse);
    }
}
