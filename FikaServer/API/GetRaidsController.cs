using FikaServer.Models;
using FikaServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/raids")]
[RequireApiKey]
public class GetRaidsController(MatchService matchService) : ControllerBase
{
    [HttpGet]
    public IActionResult HandleRequest()
    {
        return Ok(matchService.Matches);
    }
}

