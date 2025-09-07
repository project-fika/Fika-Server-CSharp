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

namespace FikaServer.Http.Post;

[Injectable(TypePriority = 0)]
public class HttpAddFleaBan(SaveServer saveServer, ConfigService configService,
    TimeUtil timeUtil, JsonUtil jsonUtil, NotificationSendHelper sendHelper) : BaseHttpRequest(configService)
{
    public override string Path { get; set; } = "/post/addfleaban";

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

            AddFleaBanRequest? request = jsonUtil.Deserialize<AddFleaBanRequest>(rawData);
            if (request != null)
            {
                MongoId profileId = new(request.ProfileId);
                SptProfile profile = saveServer.GetProfile(profileId);
                if (profile != null)
                {
                    int days = request.AmountOfDays == 0 ? 9999 : request.AmountOfDays;
                    long banTime = timeUtil.GetTimeStampFromNowDays(days);
                    profile.CharacterData.PmcData.Info.Bans = (profile.CharacterData.PmcData.Info.Bans ?? [])
                        .Append(new Ban()
                        {
                            BanType = BanType.RagFair,
                            DateTime = banTime
                        });

                    await saveServer.SaveProfileAsync(profileId);

                    sendHelper.SendMessage(profileId, new AddBanNotification()
                    {
                        EventType = NotificationEventType.InGameBan,
                        EventIdentifier = new(),
                        BanType = BanType.RagFair,
                        DateTime = banTime
                    });
                }
            }
        }

        resp.StatusCode = 200;
        await resp.StartAsync();
        await resp.CompleteAsync();
    }
}
