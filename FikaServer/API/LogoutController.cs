using FikaServer.Models;
using FikaShared.Requests;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/logout")]
[RequireApiKey]
public class LogoutController(NotificationSendHelper sendHelper) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandlePostRequest([FromBody] ProfileIdRequest request)
    {
        MongoId profileId = new(request.ProfileId);

        sendHelper.SendMessage(profileId, new WsNotificationEvent()
        {
            EventType = NotificationEventType.ForceLogout,
            EventIdentifier = new()
        });

        return Ok();
    }
}
