using FikaServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/heartbeat")]
[RequireApiKey]
public class HeartbeatController() : ControllerBase
{
    [HttpGet]
    public IActionResult HandleRequest()
    {
        return Ok();
    }
}
