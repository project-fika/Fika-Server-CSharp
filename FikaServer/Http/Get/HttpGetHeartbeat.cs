using FikaServer.Services;
using SPTarkov.DI.Annotations;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetHeartbeat(ConfigService configService) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/heartbeat";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
