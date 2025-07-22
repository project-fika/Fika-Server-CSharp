using FikaServer.Models.Fika;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetRaids(ConfigService configService, MatchService matchService, HttpResponseUtil httpResponseUtil) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/raids";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            ConcurrentDictionary<MongoId, FikaMatch> matches = matchService.Matches;

            resp.StatusCode = 200;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(matches)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
