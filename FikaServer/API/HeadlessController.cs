using System.Net.WebSockets;
using FikaServer.Helpers;
using FikaServer.Models;
using FikaServer.Services.Headless;
using FikaShared;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using static FikaShared.Enums;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/headless")]
[RequireApiKey]
public class HeadlessController(HeadlessService headlessService, HeadlessHelper headlessHelper) : ControllerBase
{
    [HttpGet]
    public IActionResult HandleRequest()
    {
        var headlessClients = headlessService.HeadlessClients;
        var clients = new List<OnlineHeadless>(headlessClients.Count);
        foreach ((var profileId, var headlessClient) in headlessClients)
        {
            var state = headlessClient.WebSocket.State is WebSocketState.Open ? EHeadlessState.Ready : EHeadlessState.NotReady;
            clients.Add(new()
            {
                ProfileId = profileId,
                Nickname = headlessHelper.GetHeadlessNickname(profileId),
                State = state,
                Players = (headlessClient.Players?.Count) ?? 0
            });
        }

        var headlessResponse = new GetHeadlessResponse()
        {
            HeadlessClients = clients
        };

        return Ok(headlessResponse);
    }
}
