using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetProfile(SaveServer saveServer, HttpResponseUtil httpResponseUtil,
        ConfigService configService, JsonUtil jsonUtil) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/rawprofile";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            using (StreamReader sr = new(req.Body))
            {
                string rawData = await sr.ReadToEndAsync();

                string mongoId = req.Query["profileId"];

                if (mongoId == null || !mongoId.IsValidMongoId())
                {
                    resp.StatusCode = 400;
                    await resp.StartAsync();
                    await resp.CompleteAsync();

                    return;
                }

                MongoId profileId = new(mongoId);
                SptProfile profile = saveServer.GetProfile(profileId);
                if (profile != null)
                {
                    resp.StatusCode = 200;
                    await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(jsonUtil.Serialize(profile, true)));
                    await resp.StartAsync();
                    await resp.CompleteAsync();

                    return;
                }
            }

            resp.StatusCode = 404;
            resp.ContentType = ContentTypes.Json;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
