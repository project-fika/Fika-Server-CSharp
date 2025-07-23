using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Post
{
    [Injectable(TypePriority = 0)]
    public class HttpUploadProfile(ConfigService configService, JsonUtil jsonUtil,
        SaveServer saveServer, ProfileActivityService profileActivityService) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/post/uploadprofile";

        public override string Method
        {
            get
            {
                return HttpMethods.Post;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            try
            {
                using (StreamReader sr = new(req.Body))
                {
                    string rawData = await sr.ReadToEndAsync();
                    SptProfile? profile = jsonUtil.Deserialize<SptProfile>(rawData);

                    if (profile != null)
                    {
                        if (!profile.HasProfileData())
                        {
                            resp.StatusCode = 400;
                            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"Profile is missing data"));
                            await resp.StartAsync();
                            await resp.CompleteAsync();

                            return;
                        }

                        var profileId = profile.ProfileInfo.ProfileId.GetValueOrDefault();

                        var existingProfile = saveServer.GetProfiles().Values;
                        if (existingProfile.Any(p => p.HasProfileData() && p.ProfileInfo.ProfileId == profileId))
                        {
                            // profile is active, we cannot update it
                            if (profileActivityService.ActiveWithinLastMinutes(profileId, 5))
                            {
                                resp.StatusCode = 423;
                                await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"'{profileId}' has been logged in within the last 5 minutes, cannot update"));
                                await resp.StartAsync();
                                await resp.CompleteAsync();

                                return;
                            }

                            // profile already exists, update it, and exit on failure
                            if (!saveServer.RemoveProfile(profileId))
                            {
                                resp.StatusCode = 500;
                                await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"'{profileId}' already exists but could not be removed"));
                                await resp.StartAsync();
                                await resp.CompleteAsync();

                                return;
                            }

                            saveServer.AddProfile(profile);
                            await saveServer.SaveProfileAsync(profileId);

                            resp.StatusCode = 200;
                            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"Profile for {profile.CharacterData.PmcData.Info.Nickname} was updated successfully"));
                            await resp.StartAsync();
                            await resp.CompleteAsync();

                            return;
                        }

                        if (existingProfile.Any(p => p.HasProfileData() && p.CharacterData.PmcData.Info.Nickname == profile.CharacterData.PmcData.Info.Nickname))
                        {
                            resp.StatusCode = 400;
                            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"A profile with the nickname '{profile.CharacterData.PmcData.Info.Nickname}' already exists or nickname was already taken"));
                            await resp.StartAsync();
                            await resp.CompleteAsync();

                            return;
                        }

                        saveServer.AddProfile(profile);
                        await saveServer.SaveProfileAsync(profileId);

                        resp.StatusCode = 200;
                        await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"Profile for {profile.CharacterData.PmcData.Info.Nickname} was uploaded successfully"));
                        await resp.StartAsync();
                        await resp.CompleteAsync();
                    }
                    else
                    {
                        resp.StatusCode = 400;
                        await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"Profile could not be deserialized"));
                        await resp.StartAsync();
                        await resp.CompleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                resp.StatusCode = 400;
                await resp.Body.WriteAsync(Encoding.UTF8.GetBytes($"There was an error uploading the profile: {ex.Message}"));
                await resp.StartAsync();
                await resp.CompleteAsync();
            }
        }
    }
}
