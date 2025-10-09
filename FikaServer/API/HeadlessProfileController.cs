using FikaServer.Models;
using FikaServer.Services.Headless;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/createheadlessprofile")]
[RequireApiKey]
public class HeadlessProfileController(HeadlessProfileService headlessProfileService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandlePostRequest()
    {
        if (headlessProfileService == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Headless profile service not available.");
        }

        var headlessProfiles = await headlessProfileService.CreateHeadlessProfiles(1, true);

        if (headlessProfiles == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate headless profile.");
        }

        if (headlessProfiles.Count == 0)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate headless profile.");
        }

        var headlessProfile = headlessProfiles[0];

        if (headlessProfile == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate headless profile.");
        }

        string? headlessProfileId = headlessProfile.ProfileInfo?.ProfileId;

        if (headlessProfileId == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Generated profile id is null.");
        }

        CreateHeadlessProfileResponse createHeadlessProfileResponse = new()
        {
            Id = headlessProfileId
        };

        return Ok(createHeadlessProfileResponse);
    }
}
