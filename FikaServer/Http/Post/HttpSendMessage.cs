using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaShared.Requests;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Http.Post;

[Injectable(TypePriority = 0)]
public class HttpSendMessage(ConfigService configService, JsonUtil jsonUtil, NotificationSendHelper sendHelper) : BaseHttpRequest(configService)
{
    public override string Path { get; set; } = "/post/sendmessage";

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

            SendMessageRequest request = jsonUtil.Deserialize<SendMessageRequest>(rawData);
            if (request != null)
            {
                MongoId profileId = new(request.ProfileId);
                sendHelper.SendMessage(profileId, new SendMessageNotification(request.Message)
                {
                    EventType = NotificationEventType.tournamentWarning,
                    EventIdentifier = new()
                });
            }
        }

        resp.StatusCode = 200;
        await resp.StartAsync();
        await resp.CompleteAsync();
    }
}
