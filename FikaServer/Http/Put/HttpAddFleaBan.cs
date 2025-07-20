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
    public class HttpAddFleaBan(SaveServer saveServer, HttpResponseUtil httpResponseUtil, ConfigService configService, TimeUtil timeUtil) : IHttpListener
    {
        public bool CanHandle(MongoId sessionId, HttpRequest req)
        {
            if (req.Method != HttpMethods.Put)
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
            using (var sr = new StreamReader(req.Body))
            {
                var rawData = await sr.ReadToEndAsync();

                AddFleaBanRequest request = JsonSerializer.Deserialize<AddFleaBanRequest>(rawData);
                if (request != null)
                {
                    var profile = saveServer.GetProfile(sessionId);
                    if (profile != null)
                    {
                        long banTime = timeUtil.GetTimeStampFromNowDays(1);
                        profile.CharacterData?.PmcData?.Info?.Bans?.Add(new()
                        {
                            BanType = BanType.RagFair,
                            DateTime = banTime
                        });
                    }
                }
            }

            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody("OK")));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
