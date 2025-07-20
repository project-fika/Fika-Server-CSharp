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
using System.Text.Json;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpAddFleaBan(SaveServer saveServer, HttpResponseUtil httpResponseUtil, ConfigService configService, TimeUtil timeUtil, JsonUtil jsonUtil) : IHttpListener
    {
        public bool CanHandle(MongoId sessionId, HttpRequest req)
        {
            if (req.Method != HttpMethods.Post)
            {
                return false;
            }

            if (!req.Path.Value?.Contains("put/addfleaban", StringComparison.OrdinalIgnoreCase) ?? true)
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
            using (StreamReader sr = new(req.Body))
            {
                string rawData = await sr.ReadToEndAsync();

                AddFleaBanRequest request = jsonUtil.Deserialize<AddFleaBanRequest>(rawData);
                if (request != null)
                {
                    SptProfile profile = saveServer.GetProfile(request.ProfileId);
                    if (profile != null)
                    {
                        long banTime = timeUtil.GetTimeStampFromNowDays(1);
                        profile.CharacterData?.PmcData?.Info?.Bans?.Add(new()
                        {
                            BanType = BanType.RagFair,
                            DateTime = banTime
                        });

                        await saveServer.SaveProfileAsync(request.ProfileId);
                    }
                }
            }

            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
