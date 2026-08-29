using System.Net;
using FikaShared.Requests;
using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Moderator")]
[ApiController]
[Route("api/[controller]")]
public sealed class ToolsController(
    IHttpClientFactory httpClientFactory,
    ItemCacheService itemCacheService,
    SendTimersService sendTimersService,
    ILogger<ToolsController> logger) : ControllerBase
{
    [HttpPost("senditemtoall")]
    public async Task<ActionResult<MessageResponseDto>> SendItemToAll([FromBody] SendItemToAllRequest request)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("fika/api/senditemtoall", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new MessageResponseDto(error));
            }

            return Ok(new MessageResponseDto("Item sent to everyone"));
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new MessageResponseDto("Something went wrong when sending the item: [403 Forbidden].\nAre you using the wrong API key?"));
            }

            if (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, new MessageResponseDto("Something went wrong when sending the item: [404 NotFound].\nAre you missing the Fika server mod?"));
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponseDto(httpEx.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending item to all.");
            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponseDto(ex.Message));
        }
    }

    [HttpGet("items/search")]
    public ActionResult<IEnumerable<ItemSearchResultDto>> SearchItems([FromQuery] string? query)
    {
        var results = itemCacheService.NameToIdSearch(query ?? string.Empty, 25);
        return Ok(results);
    }

    [HttpGet("items/resolve/{templateId}")]
    public ActionResult<ResolvedItemDto> ResolveItem(string templateId)
    {
        if (!itemCacheService.TryGetItem(templateId, out var data) || data == null)
        {
            return NotFound();
        }

        var response = new ResolvedItemDto(
            TemplateId: templateId,
            Name: data.Name,
            Description: data.Description,
            MaxItems: Math.Clamp(data.StackAmount, 1, 5_000_000)
        );

        return Ok(response);
    }

    [HttpPost("items/refresh")]
    public async Task<ActionResult<MessageResponseDto>> RefreshItemCache()
    {
        var success = await itemCacheService.PopulateDictionary();
        return success
            ? Ok(new MessageResponseDto("Items successfully refreshed"))
            : StatusCode(StatusCodes.Status500InternalServerError, new MessageResponseDto("There was an error refreshing the database"));
    }

    [HttpGet("queued")]
    public ActionResult<QueuedItemsResponseDto> GetQueuedItems()
    {
        var singleTimers = sendTimersService.Timers.Select(pair => new QueuedSingleTimerDto(
            Ticks: pair.Value.SendDate.GetValueOrDefault().Ticks,
            ProfileId: pair.Value.ProfileId,
            ItemTemplate: pair.Value.ItemTemplate,
            ItemName: itemCacheService.IdToName(pair.Value.ItemTemplate)?.Name ?? pair.Value.ItemTemplate,
            Amount: pair.Value.Amount,
            Message: pair.Value.Message,
            FoundInRaid: pair.Value.FoundInRaid,
            SendDate: pair.Value.SendDate.GetValueOrDefault().ToString("o")
        ));

        var allTimers = sendTimersService.ToAllTimers.Select(pair => new QueuedAllTimerDto(
            Ticks: pair.Value.SendDate.GetValueOrDefault().Ticks,
            ItemTemplate: pair.Value.ItemTemplate,
            ItemName: itemCacheService.IdToName(pair.Value.ItemTemplate)?.Name ?? pair.Value.ItemTemplate,
            Amount: pair.Value.Amount,
            Message: pair.Value.Message,
            FoundInRaid: pair.Value.FoundInRaid,
            ProfileIds: pair.Value.ProfileIds,
            SendDate: pair.Value.SendDate.GetValueOrDefault().ToString("o")
        ));

        return Ok(new QueuedItemsResponseDto(singleTimers, allTimers));
    }

    [HttpPost("queued/delete")]
    public ActionResult<MessageResponseDto> DeleteQueuedItem([FromBody] DeleteTimerRequestDto request)
    {
        var singleMatch = sendTimersService.Timers.FirstOrDefault(x => x.Value.SendDate.GetValueOrDefault().Ticks == request.Ticks);
        if (singleMatch.Key != null)
        {
            sendTimersService.RemoveTimer(singleMatch.Key);
            return Ok(new MessageResponseDto("Queued item deleted"));
        }

        var allMatch = sendTimersService.ToAllTimers.FirstOrDefault(x => x.Value.SendDate.GetValueOrDefault().Ticks == request.Ticks);
        if (allMatch.Key != null)
        {
            sendTimersService.RemoveTimer(allMatch.Key);
            return Ok(new MessageResponseDto("Queued item deleted"));
        }

        return NotFound(new MessageResponseDto("Queued item not found"));
    }

    [HttpPost("schedule/single")]
    public ActionResult<MessageResponseDto> ScheduleSingle([FromBody] ScheduleSingleRequestDto payload)
    {
        sendTimersService.AddTimer(payload.Request, payload.SendDate);
        return Ok(new MessageResponseDto($"Item queued for {payload.SendDate}"));
    }

    [HttpPost("schedule/all")]
    public ActionResult<MessageResponseDto> ScheduleAll([FromBody] ScheduleAllRequestDto payload)
    {
        sendTimersService.AddTimer(payload.Request, payload.SendDate);
        return Ok(new MessageResponseDto($"Item queued for all users at {payload.SendDate}"));
    }
}