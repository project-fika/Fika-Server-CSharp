using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaShared.Requests;
using Microsoft.Extensions.Primitives;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Http;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Http.Post
{
    [Injectable(TypePriority = 0)]
    public class HttpAddRemoveBan(SaveServer saveServer, ConfigService configService,
        JsonUtil jsonUtil, NotificationSendHelper sendHelper) : IHttpListener
    {
        public bool CanHandle(MongoId sessionId, HttpRequest req)
        {
            if (req.Method != HttpMethods.Post)
            {
                return false;
            }

            if (!req.Path.Value?.Contains("put/removefleaban", StringComparison.OrdinalIgnoreCase) ?? true)
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
            resp.StatusCode = 403;
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

                        resp.StatusCode = 200;
                    }
                }
            }

            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
