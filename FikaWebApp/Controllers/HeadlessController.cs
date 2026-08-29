using System.Net;
using FikaShared;
using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class HeadlessController(
    IHttpClientFactory httpClientFactory,
    ILogger<HeadlessController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OnlineHeadless>>> GetHeadless()
    {
#if DEBUG
        await Task.Delay(TimeSpan.FromSeconds(1));
        List<OnlineHeadless> mockClients =
        [
            new()
            {
                ProfileId = "TEST",
                Nickname = "TEST",
                State = Enums.EHeadlessState.Ready,
                Players = Random.Shared.Next(0, 5),
                ProfileIds = []
            },
            new()
            {
                ProfileId = "TEST2",
                Nickname = "TEST2",
                State = Enums.EHeadlessState.NotReady,
                Players = Random.Shared.Next(0, 5),
                ProfileIds = []
            },
            new()
            {
                ProfileId = "TEST3",
                Nickname = "TEST3",
                State = Enums.EHeadlessState.NotReady,
                Players = Random.Shared.Next(0, 5),
                ProfileIds = []
            },
            new()
            {
                ProfileId = "TEST4",
                Nickname = "TEST4",
                State = Enums.EHeadlessState.Ready,
                Players = Random.Shared.Next(0, 5),
                ProfileIds = []
            }
        ];
        return Ok(mockClients);
#else
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<GetHeadlessResponse>("fika/api/headless");
            return Ok(response?.HeadlessClients ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching headless clients");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error retrieving the data: {ex.Message}"));
        }
#endif
    }

    [HttpPost("restart")]
    public async Task<ActionResult<ApiResponseDto>> RestartHeadless([FromBody] ProfileIdRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.PostAsJsonAsync("fika/api/restartheadless", request);

            if (!result.IsSuccessStatusCode)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto($"There was an error returned from the server: StatusCode {result.StatusCode}"));
            }

            return Ok(new ApiResponseDto("Headless client restarted successfully."));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Something went wrong when sending restart request: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when sending the restart request: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Something went wrong when sending restart request: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when sending the restart request: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when sending restart request.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error restarting headless client.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error when sending the request: {ex.Message}"));
        }
    }
}