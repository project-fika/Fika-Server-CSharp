using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaShared;
using Microsoft.Extensions.Primitives;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetProfiles(SaveServer saveServer, HttpResponseUtil httpResponseUtil, ConfigService configService) : IHttpListener
    {
        public bool CanHandle(MongoId sessionId, HttpRequest req)
        {
            if (req.Method != HttpMethods.Get)
            {
                return false;
            }

            if (!req.Path.Value?.Contains("get/profiles", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                return false;
            }

            if (!req.Headers.TryGetValue("Auth", out StringValues authHeader))
            {
                return false;
            }

            return authHeader.Contains(configService.Config.Server.ApiKey);
        }

        public async Task Handle(MongoId sessionId, HttpRequest req, HttpResponse resp)
        {
            var profiles = saveServer.GetProfiles().Values;
            List<ProfileResponse> profilesResponse = [];
            foreach (SptProfile profile in profiles)
            {
                profilesResponse.Add(new()
                {
                    Nickname = profile.CharacterData.PmcData.Info.Nickname,
                    ProfileId = profile.ProfileInfo.ProfileId,
                    HasFleaBan = profile.CharacterData.PmcData.Info.Bans.Any(x => x.BanType is BanType.RagFair)
                });
            }

            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(profilesResponse)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
