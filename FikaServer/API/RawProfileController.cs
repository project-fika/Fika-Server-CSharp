using System.Text;
using FikaServer.Models;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/rawprofile")]
[RequireApiKey]
public class RawProfileController(SaveServer saveServer, JsonUtil jsonUtil) : ControllerBase
{
    [HttpGet]
    public IActionResult HandleRequest([FromQuery] string profileId)
    {
        if (string.IsNullOrEmpty(profileId) || !profileId.IsValidMongoId())
        {
            return BadRequest();
        }

        var mongoId = new MongoId(profileId);
        var profile = saveServer.GetProfile(mongoId);

        if (profile != null)
        {
            var json = jsonUtil.Serialize(profile, true);
            return Content(json, "application/json", Encoding.UTF8);
        }

        return NotFound();
    }
}
