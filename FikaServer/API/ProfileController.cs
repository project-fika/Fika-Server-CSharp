using FikaServer.Models;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/profile")]
[RequireApiKey]
public sealed class ProfileController(SaveServer saveServer) : ControllerBase
{
    [HttpGet("quests")]
    public ActionResult<List<string>> GetQuests([FromQuery] string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return BadRequest(new { message = "No ProfileId provided" });
        }

        var profile = saveServer.GetProfile(profileId);
        if (profile == null)
        {
            return BadRequest(new { message = "Could not find profile with id " + profileId });
        }

        var quests = profile.CharacterData?.PmcData?.Quests?
            .Select(q => q.QId.ToString())
            .ToList();

        return Ok(quests);
    }
}
