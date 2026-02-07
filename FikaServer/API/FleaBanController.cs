using FikaServer.Models;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaShared.Requests;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/fleaban")]
[RequireApiKey]
public class FleaBanController(SaveServer saveServer, TimeUtil timeUtil, NotificationSendHelper sendHelper) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandlePostRequest([FromBody] AddFleaBanRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.ProfileId))
        {
            return BadRequest("Invalid request data.");
        }

        if (!request.ProfileId.IsValidMongoId())
        {
            return BadRequest("Invalid profile ID format.");
        }

        var profileId = new MongoId(request.ProfileId);
        var profile = saveServer.GetProfile(profileId);

        if (profile == null)
        {
            return NotFound("Profile not found.");
        }

        var days = request.AmountOfDays == 0 ? 9999 : request.AmountOfDays;
        var banTime = timeUtil.GetTimeStampFromNowDays(days);

        profile.CharacterData.PmcData.Info.Bans = (profile.CharacterData.PmcData.Info.Bans ?? [])
        .Append(new Ban()
        {
            BanType = BanType.RagFair,
            DateTime = banTime
        });

        await saveServer.SaveProfileAsync(profileId);

        sendHelper.SendMessage(profileId, new AddBanNotification
        {
            EventType = NotificationEventType.InGameBan,
            EventIdentifier = new(),
            BanType = BanType.RagFair,
            DateTime = banTime
        });

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> HandleDeleteRequest([FromBody] ProfileIdRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.ProfileId))
        {
            return BadRequest("Invalid request.");
        }

        if (!request.ProfileId.IsValidMongoId())
        {
            return BadRequest("Invalid profile ID format.");
        }

        var profileId = new MongoId(request.ProfileId);
        var profile = saveServer.GetProfile(profileId);

        if (profile == null)
        {
            return NotFound("Profile not found.");
        }

        var bans = profile.CharacterData?.PmcData?.Info?.Bans;
        if (bans != null)
        {
            profile.CharacterData.PmcData.Info.Bans = bans
                .Where(b => b.BanType is not BanType.RagFair)
                .ToList();
        }

        await saveServer.SaveProfileAsync(profileId);

        sendHelper.SendMessage(profileId, new RemoveBanNotification
        {
            EventType = NotificationEventType.InGameUnBan,
            EventIdentifier = new(),
            BanType = BanType.RagFair
        });

        return Ok();
    }
}
