using FikaServer.Models;
using FikaServer.Models.Fika.Headless;
using FikaServer.Services.Headless;
using FikaShared.Requests;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/restartheadless")]
[RequireApiKey]
public class RestartHeadlessController(JsonUtil jsonUtil, HeadlessService headlessService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandlePostRequest([FromBody] ProfileIdRequest request)
    {
        MongoId profileId = new(request.ProfileId);

        if (headlessService.HeadlessClients.TryGetValue(profileId, out HeadlessClientInfo? client))
        {
            if (client.WebSocket == null || client.WebSocket.State is WebSocketState.Closed)
            {
                return NotFound();
            }

            string? data = jsonUtil.Serialize(new HeadlessShutdownClient())
                ?? throw new NullReferenceException("ShutdownClient::Data was null after serializing");
            await client.WebSocket.SendAsync(Encoding.UTF8.GetBytes(data),
            WebSocketMessageType.Text, true, CancellationToken.None);
        }

        return Ok();
    }
}
