using FikaServer.Models;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaShared.Requests;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/sendmessage")]
[RequireApiKey]
public class SendMessageController(NotificationSendHelper sendHelper) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandlePostRequest([FromBody] SendMessageRequest request)
    {
        MongoId profileId = new(request.ProfileId);

        await sendHelper.SendMessageAsync(profileId, new SendMessageNotification(request.Message)
        {
            EventType = NotificationEventType.tournamentWarning,
            EventIdentifier = new()
        });

        return Ok();
    }
}
