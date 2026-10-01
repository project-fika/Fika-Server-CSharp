using System.Net;
using FikaShared;
using FikaShared.Enums;
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
public sealed class PlayersController(
    IHttpClientFactory httpClientFactory,
    ILogger<PlayersController> logger) : ControllerBase
{
    public sealed record SendMessagePayload(string ProfileId, string Message);

    [HttpGet]
    public async Task<IActionResult> GetPlayers()
    {
#if DEBUG
        await Task.Delay(TimeSpan.FromSeconds(1));
        List<OnlinePlayer> mockPlayers =
        [
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.Labyrinth, Nickname = "John", ProfileId = "test1" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.Customs, Nickname = "West", ProfileId = "test2" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.Streets, Nickname = "Bjorn", ProfileId = "test3" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.Hideout, Nickname = "Roland", ProfileId = "test4" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.None, Nickname = "TarkovMan1337", ProfileId = "test5" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.Woods, Nickname = "Janky", ProfileId = "test6" },
            new() { Level = Random.Shared.Next(1, 69), Location = EFikaLocation.GroundZero, Nickname = "guidot", ProfileId = "test7" }
        ];
        return Ok(mockPlayers);
#else
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<GetOnlinePlayersResponse>("fika/api/players");
            return Ok(response?.Players ?? []);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Something went wrong when querying for heartbeat: [403 Forbidden]. Are you using the wrong API key?");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when querying for heartbeat: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Something went wrong when querying for heartbeat: [404 NotFound]. Are you missing the Fika server mod?");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when querying for heartbeat: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when querying for heartbeat.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving players");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error retrieving the players: {ex.Message}"));
        }
#endif
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessagePayload payload)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var request = new SendMessageRequest { ProfileId = payload.ProfileId, Message = payload.Message };
            var result = await client.PostAsJsonAsync("fika/api/sendmessage", request);
            result.EnsureSuccessStatusCode();

            return Ok(new ApiResponseDto("Message sent successfully"));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Something went wrong when sending the message: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when sending the message: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Something went wrong when sending the message: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when sending the message: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when sending message.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"Failed to send message: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutPlayer([FromBody] ProfileIdRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.PostAsJsonAsync("fika/api/logout", request);
            result.EnsureSuccessStatusCode();

            return Ok(new ApiResponseDto("Sent logout request successfully"));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Something went wrong when sending the logout request: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when sending the logout request: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Something went wrong when sending the logout request: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when sending the logout request: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when sending logout request.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send logout request.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"Failed to send logout request: {ex.Message}"));
        }
    }
}