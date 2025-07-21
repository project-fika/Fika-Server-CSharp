using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaShared.Requests;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Http.Post
{
    [Injectable(TypePriority = 0)]
    public class HttpAddRemoveBan(SaveServer saveServer, ConfigService configService,
        JsonUtil jsonUtil, NotificationSendHelper sendHelper) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "post/removefleaban";

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
                    SptProfile profile = saveServer.GetProfile(profileId);
                    if (profile != null)
                    {
                        profile.CharacterData?.PmcData?.Info?.Bans?.RemoveAll(b => b.BanType is BanType.RagFair);

                        await saveServer.SaveProfileAsync(profileId);

                        sendHelper.SendMessage(profileId, new RemoveBanNotification()
                        {
                            EventType = NotificationEventType.InGameUnBan,
                            EventIdentifier = new(),
                            BanType = BanType.RagFair
                        });
                    }
                }
            }

            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
