using FikaServer.Models.Fika.Headless;
using FikaServer.Services;
using FikaServer.Services.Headless;
using FikaShared;
using FikaShared.Requests;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.Http.Post;

[Injectable]
public class HttpCreateHeadlessProfile(ConfigService configService, HeadlessProfileService headlessProfileService, 
    HttpResponseUtil httpResponseUtil) : BaseHttpRequest(configService)
{
    public override string Path { get; set; } = "/post/createheadlessprofile";

    public override string Method
    {
        get
        {
            return HttpMethods.Post;
        }
    }

    public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
    {
        if (headlessProfileService == null)
        {
            resp.StatusCode = StatusCodes.Status500InternalServerError;
            await resp.StartAsync();
            await resp.CompleteAsync();

            return;
        }

        SptProfile headlessProfile = await headlessProfileService.CreateHeadlessProfile();

        if (headlessProfile == null)
        {
            resp.StatusCode = StatusCodes.Status500InternalServerError;
            await resp.StartAsync();
            await resp.CompleteAsync();

            return;
        }

        string? headlessProfileId = headlessProfile.ProfileInfo?.ProfileId;

        if (headlessProfileId == null)
        {
            resp.StatusCode = StatusCodes.Status500InternalServerError;
            await resp.StartAsync();
            await resp.CompleteAsync();

            return;
        }

        headlessProfileService.HeadlessProfiles.Add(headlessProfile);
        headlessProfileService.GenerateLaunchScript(headlessProfileId);

        CreateHeadlessProfileResponse createHeadlessProfileResponse = new()
        {
            Id = headlessProfileId
        };

        resp.StatusCode = StatusCodes.Status200OK;
        resp.ContentType = ContentTypes.Json;

        await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(createHeadlessProfileResponse)));
        await resp.StartAsync();
        await resp.CompleteAsync();
    }
}
