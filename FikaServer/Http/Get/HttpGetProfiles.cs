using FikaServer.Services;
using FikaServer.Services.Cache;
using Microsoft.Extensions.Primitives;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetProfiles(FikaProfileService fikaProfileService, HttpResponseUtil httpResponseUtil, ConfigService configService) : IHttpListener
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
            var profiles = fikaProfileService.GetAllProfiles();

            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(profiles)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
