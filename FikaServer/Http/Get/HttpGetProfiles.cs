using FikaServer.Services;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetProfiles(SaveServer saveServer, HttpResponseUtil httpResponseUtil, ConfigService configService) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "get/profiles";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
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

            resp.StatusCode = 200;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(profilesResponse)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
