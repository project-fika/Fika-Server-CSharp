using System.Net;
using FikaShared.Responses;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class StatisticsController(
    IHttpClientFactory httpClientFactory,
    ILogger<StatisticsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StatisticsPlayer>>> GetStatistics()
    {
#if DEBUG
        await Task.Delay(TimeSpan.FromSeconds(1));
        List<StatisticsPlayer> mockPlayers =
        [
            new()
            {
                Nickname = "Test1",
                Kills = Random.Shared.NextDouble() * 10_000,
                Deaths = Random.Shared.NextDouble() * 1000,
                AmmoUsed = Random.Shared.NextDouble() * 100_000,
                BodyDamage = Random.Shared.NextDouble() * 100_000,
                ArmorDamage = Random.Shared.NextDouble() * 100_000,
                Headshots = Random.Shared.NextDouble() * 100,
                BossKills = Random.Shared.NextDouble() * 100
            },
            new()
            {
                Nickname = "Test2",
                Kills = Random.Shared.NextDouble() * 10_000,
                Deaths = Random.Shared.NextDouble() * 1000,
                AmmoUsed = Random.Shared.NextDouble() * 100_000,
                BodyDamage = Random.Shared.NextDouble() * 100_000,
                ArmorDamage = Random.Shared.NextDouble() * 100_000,
                Headshots = Random.Shared.NextDouble() * 100,
                BossKills = Random.Shared.NextDouble() * 100
            },
            new()
            {
                Nickname = "Test3",
                Kills = Random.Shared.NextDouble() * 10_000,
                Deaths = Random.Shared.NextDouble() * 1000,
                AmmoUsed = Random.Shared.NextDouble() * 100_000,
                BodyDamage = Random.Shared.NextDouble() * 100_000,
                ArmorDamage = Random.Shared.NextDouble() * 100_000,
                Headshots = Random.Shared.NextDouble() * 100,
                BossKills = Random.Shared.NextDouble() * 100
            },
            new()
            {
                Nickname = "Test4",
                Kills = Random.Shared.NextDouble() * 10_000,
                Deaths = Random.Shared.NextDouble() * 1000,
                AmmoUsed = Random.Shared.NextDouble() * 100_000,
                BodyDamage = Random.Shared.NextDouble() * 100_000,
                ArmorDamage = Random.Shared.NextDouble() * 100_000,
                Headshots = Random.Shared.NextDouble() * 100,
                BossKills = Random.Shared.NextDouble() * 100
            },
            new()
            {
                Nickname = "Test5",
                Kills = Random.Shared.NextDouble() * 10_000,
                Deaths = Random.Shared.NextDouble() * 1000,
                AmmoUsed = Random.Shared.NextDouble() * 100_000,
                BodyDamage = Random.Shared.NextDouble() * 100_000,
                ArmorDamage = Random.Shared.NextDouble() * 100_000,
                Headshots = Random.Shared.NextDouble() * 100,
                BossKills = Random.Shared.NextDouble() * 100
            }
        ];

        return Ok(mockPlayers);
#else
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<GetStatisticsResponse>("fika/api/statistics");
            return Ok(response?.Players ?? []);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Error retrieving statistics: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when retrieving statistics: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Error retrieving statistics: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when retrieving statistics: [404 NotFound]. Are you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when retrieving statistics.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving statistics.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error retrieving statistics: {ex.Message}"));
        }
#endif
    }
}