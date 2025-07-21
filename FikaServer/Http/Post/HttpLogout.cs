using FikaServer.Services;
using FikaShared.Requests;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Http.Post
{
    public class HttpLogout(ConfigService configService, JsonUtil jsonUtil, NotificationSendHelper sendHelper) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "post/logout";

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
                    sendHelper.SendMessage(profileId, new WsNotificationEvent()
                    {
                        EventType = NotificationEventType.ForceLogout,
                        EventIdentifier = new()
                    });
                }
            }

            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
