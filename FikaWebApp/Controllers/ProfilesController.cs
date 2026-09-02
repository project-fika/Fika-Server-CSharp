using System.Net;
using System.Text;
using FikaShared.Enums;
using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Moderator")]
[ApiController]
[Route("api/[controller]")]
public sealed class ProfilesController(
    DataCacheService dataCacheService,
    IHttpClientFactory httpClientFactory,
    ILogger<ProfilesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileResponse>>> GetProfiles()
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<List<ProfileResponse>>("fika/api/profiles");
            return Ok(response ?? []);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Error retrieving profiles: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when retrieving profiles: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Error retrieving profiles: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when retrieving profiles: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when retrieving profiles.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving profiles.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"An error occurred: {ex.Message}"));
        }
    }

    [HttpGet("raw")]
    public async Task<ActionResult<string>> GetRawProfile([FromQuery] string profileId)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.GetStringAsync($"fika/api/rawprofile?profileId={Uri.EscapeDataString(profileId)}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving raw profile for {ProfileId}", profileId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error retrieving data: {ex.Message}"));
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadProfile([FromBody] string profileJson)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.PostAsync("fika/api/uploadprofile", new StringContent(profileJson, Encoding.UTF8, "application/json"));
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                return Ok(content);
            }

            return StatusCode((int)result.StatusCode, new ApiResponseDto(content));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading profile.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"Failed to upload profile: {ex.Message}"));
        }
    }

    [HttpPost("add-flea-ban")]
    public async Task<ActionResult<ApiResponseDto>> AddFleaBan([FromBody] AddFleaBanRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.PostAsJsonAsync("fika/api/fleaban", request);

            if (!result.IsSuccessStatusCode)
            {
                var error = await result.Content.ReadAsStringAsync();
                return StatusCode((int)result.StatusCode, new ApiResponseDto(error));
            }

            return Ok(new ApiResponseDto("Flea ban added successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding flea ban.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto(ex.Message));
        }
    }

    [HttpPost("remove-flea-ban")]
    public async Task<ActionResult<ApiResponseDto>> RemoveFleaBan([FromBody] ProfileIdRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "fika/api/fleaban")
            {
                Content = JsonContent.Create(request)
            };

            var result = await client.SendAsync(httpRequest);

            if (!result.IsSuccessStatusCode)
            {
                var error = await result.Content.ReadAsStringAsync();
                return StatusCode((int)result.StatusCode, new ApiResponseDto(error));
            }

            return Ok(new ApiResponseDto("Flea ban removed successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing flea ban.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto(ex.Message));
        }
    }

    [HttpPost("send-item")]
    public async Task<ActionResult<ApiResponseDto>> SendItem([FromBody] SendItemRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var result = await client.PostAsJsonAsync("fika/api/senditem", request);

            if (!result.IsSuccessStatusCode)
            {
                var errorMessage = await result.Content.ReadAsStringAsync();
                return StatusCode((int)result.StatusCode, new ApiResponseDto($"[{result.StatusCode}] {errorMessage}"));
            }

            return Ok(new ApiResponseDto("Item sent successfully"));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Error sending item: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when sending the item: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Error sending item: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when sending the item: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when sending item.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending item.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"There was an error sending the item: {ex.Message}"));
        }
    }

    [HttpGet("quests")]
    public async Task<ActionResult<List<QuestData>>> GetQuests([FromQuery] string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return BadRequest(new ApiResponseDto("No ProfileId provided"));
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var activeQuests = await client.GetFromJsonAsync<List<QuestData>>($"fika/api/profile/quests?profileId={Uri.EscapeDataString(profileId)}");
            if (activeQuests == null || activeQuests.Count == 0)
            {
                return Ok(new List<QuestData>());
            }

            foreach (var activeQuest in activeQuests)
            {
                if (string.IsNullOrWhiteSpace(activeQuest.Id) ||
                    !dataCacheService.TryGetQuest(activeQuest.Id, out var cachedQuest) ||
                    cachedQuest == null)
                {
                    continue;
                }

                // Hydrate top-level quest metadata
                activeQuest.Name = cachedQuest.Name;
                activeQuest.Description = cachedQuest.Description;
                activeQuest.ItemRewards = cachedQuest.ItemRewards;
                activeQuest.TraderRewards = cachedQuest.TraderRewards;
                activeQuest.ExperienceRewards = cachedQuest.ExperienceRewards;

                // Hydrate objective descriptions or fallback if objectives list is empty
                if (activeQuest.Objectives?.Count > 0)
                {
                    foreach (var activeObj in activeQuest.Objectives)
                    {
                        var staticObj = cachedQuest.Objectives?.FirstOrDefault(o => o.Id == activeObj.Id);
                        if (staticObj != null)
                        {
                            activeObj.Description = staticObj.Description;
                        }
                    }
                }
                else if (cachedQuest.Objectives != null)
                {
                    // If API sent no objective progress data, populate default uncompleted objectives
                    activeQuest.Objectives = cachedQuest.Objectives.Select(staticObj => new QuestObjective
                    {
                        Id = staticObj.Id,
                        Description = staticObj.Description,
                        Progress = 0,
                        Target = 0,
                        State = EQuestState.Started
                    }).ToList();
                }
            }

            return Ok(activeQuests);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Error retrieving profiles: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when retrieving profiles: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Error retrieving profiles: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when retrieving profiles: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when retrieving profiles.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving profiles.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("quests/complete")]
    public async Task<ActionResult<ApiResponseDto>> CompleteQuest(
        [FromQuery] string? profileId,
        [FromQuery] string? questId,
        [FromQuery] string? objectiveId = null)
    {
        if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(questId))
        {
            return BadRequest(new ApiResponseDto("Both profileId and questId are required."));
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            var targetUri = $"fika/api/profile/quests/complete?profileId={Uri.EscapeDataString(profileId)}&questId={Uri.EscapeDataString(questId)}";

            if (!string.IsNullOrWhiteSpace(objectiveId))
            {
                targetUri += $"&objectiveId={Uri.EscapeDataString(objectiveId)}";
            }

            var result = await client.PostAsync(targetUri, null);

            if (!result.IsSuccessStatusCode)
            {
                var error = await result.Content.ReadAsStringAsync();
                return StatusCode((int)result.StatusCode, new ApiResponseDto(error));
            }

            var successMessage = string.IsNullOrWhiteSpace(objectiveId)
                ? $"Quest [{questId}] completed successfully."
                : $"Objective [{objectiveId}] completed successfully for quest [{questId}].";

            return Ok(new ApiResponseDto(successMessage));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogError("Error completing quest action: [403 Forbidden]. Missing or invalid API key.");
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDto("Something went wrong when completing quest action: [403 Forbidden].\nAre you using the wrong API key?"));
            }
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogError("Error completing quest action: [404 NotFound]. Missing Fika server mod.");
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponseDto("Something went wrong when completing quest action: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            logger.LogError(httpEx, "HttpRequestException caught when completing quest action.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"HttpRequestException: {httpEx.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing quest action for ProfileId: {ProfileId}, QuestId: {QuestId}", profileId, questId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDto($"An error occurred: {ex.Message}"));
        }
    }
}