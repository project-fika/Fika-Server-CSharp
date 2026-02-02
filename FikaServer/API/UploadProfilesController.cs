using FikaServer.Models;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/uploadprofile")]
[RequireApiKey]
public class UploadProfilesController(JsonUtil jsonUtil,
    SaveServer saveServer, ProfileActivityService profileActivityService) : ControllerBase
{
    public async Task<IActionResult> HandlePostRequest()
    {
        try
        {
            using (StreamReader sr = new(Request.Body))
            {
                var rawData = await sr.ReadToEndAsync();
                var profile = jsonUtil.Deserialize<SptProfile>(rawData);

                if (profile != null)
                {
                    if (!profile.HasProfileData())
                    {
                        return BadRequest("Profile is missing data");
                    }

                    var profileId = profile.ProfileInfo.ProfileId.GetValueOrDefault();

                    var existingProfile = saveServer.GetProfiles().Values;
                    if (existingProfile.Any(p => p.HasProfileData() && p.ProfileInfo.ProfileId == profileId))
                    {
                        // profile is active, we cannot update it
                        if (profileActivityService.ActiveWithinLastMinutes(profileId, 5))
                        {
                            return StatusCode(StatusCodes.Status423Locked, $"'{profileId}' has been logged in within the last 5 minutes, cannot update");
                        }

                        // profile already exists, update it, and exit on failure
                        if (!saveServer.RemoveProfile(profileId))
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, $"'{profileId}' already exists but could not be removed");
                        }

                        saveServer.AddProfile(profile);
                        await saveServer.SaveProfileAsync(profileId);

                        return Ok($"Profile for {profile.CharacterData.PmcData.Info.Nickname} was updated successfully");
                    }

                    if (existingProfile.Any(p => p.HasProfileData() && p.CharacterData.PmcData.Info.Nickname == profile.CharacterData.PmcData.Info.Nickname))
                    {
                        return BadRequest($"A profile with the nickname '{profile.CharacterData.PmcData.Info.Nickname}' already exists or nickname was already taken");
                    }

                    saveServer.AddProfile(profile);
                    await saveServer.SaveProfileAsync(profileId);

                    return Ok($"Profile for {profile.CharacterData.PmcData.Info.Nickname} was uploaded successfully");
                }
                else
                {
                    return BadRequest("Profile could not be deserialized");
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"There was an error uploading the profile: {ex.Message}");
        }
    }
}
