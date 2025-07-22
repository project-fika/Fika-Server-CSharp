using FikaServer.Models.Fika.Headless;
using FikaServer.Services;
using FikaServer.Services.Headless;
using FikaShared.Requests;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.Http.Post
{
    [Injectable(TypePriority = 0)]
    public class HttpRestartHeadless(ConfigService configService, JsonUtil jsonUtil, NotificationSendHelper sendHelper,
        HeadlessService headlessService) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/post/restartheadless";

        public override string Method
        {
            get
            {
                return HttpMethods.Post;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            using (StreamReader sr = new(req.Body))
            {
                string rawData = await sr.ReadToEndAsync();

                ProfileIdRequest request = jsonUtil.Deserialize<ProfileIdRequest>(rawData);
                if (request != null)
                {
                    MongoId profileId = new(request.ProfileId);
                    if (headlessService.HeadlessClients.TryGetValue(profileId, out HeadlessClientInfo? client))
                    {
                        if (client.WebSocket == null || client.WebSocket.State is WebSocketState.Closed)
                        {
                            resp.StatusCode = 404;
                            await resp.StartAsync();
                            await resp.CompleteAsync();

                            return;
                        }

                        string? data = jsonUtil.Serialize(new HeadlessShutdownClient())
                            ?? throw new NullReferenceException("ShutdownClient::Data was null after serializing");
                        await client.WebSocket.SendAsync(Encoding.UTF8.GetBytes(data),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }

            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
