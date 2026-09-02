using FikaServer.Models;
using FikaShared.Enums;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Helpers.Quest;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Servers.Ws;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/profile")]
[RequireApiKey]
public sealed class ProfileController(SaveServer saveServer,
    TemplateTable templateTable, QuestHelper questHelper, SptWebSocketConnectionHandler connectionHandler) : ControllerBase
{
    [HttpGet("quests")]
    public ActionResult<List<List<QuestData>>> GetQuests([FromQuery] string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return BadRequest(new { message = "No ProfileId provided" });
        }

        var profile = saveServer.GetProfile(profileId);
        if (profile == null)
        {
            return BadRequest(new { message = "Could not find profile with id " + profileId });
        }

        var quests = profile.CharacterData?.PmcData?.Quests?
            .Select(q =>
            {
                var id = q.QId;
                var conditions = profile.CharacterData?.PmcData?.TaskConditionCounters?.Values
                    .Where(c => c.SourceId == id)
                    .ToList() ?? [];
                var conditionsData = conditions
                    .ConvertAll(c =>
                    {
                        var completed = q.Status == QuestStatusEnum.Success || q.CompletedConditions?.Contains(c.Id!) == true;
                        var state = completed ? EQuestState.Completed : (c.Value > 0d ? EQuestState.InProgress : EQuestState.Started);
                        var target = templateTable?.Quests?.Values
                            .FirstOrDefault(q => q?.Id == id)
                            ?.Conditions?.AvailableForFinish
                            ?.FirstOrDefault(tc => tc?.Id == c?.Id)
                            ?.Value ?? 0;
                        return new QuestObjective()
                        {
                            Id = c.Id!,
                            Progress = c.Value!,
                            Target = target,
                            State = state
                        };
                    })
;
                return new QuestData()
                {
                    Id = q.QId,
                    Completed = q.Status == QuestStatusEnum.Success,
                    Objectives = conditionsData
                };
            })
            .ToList();

        return Ok(quests);
    }

    [HttpPost("quests/complete")]
    public async Task<IActionResult> CompleteQuest(
        [FromQuery] string? profileId,
        [FromQuery] string? questId,
        [FromQuery] string? objectiveId = null)
    {
        if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(questId))
        {
            return BadRequest(new { message = "Both profileId and questId are required." });
        }

        if (connectionHandler.IsWebSocketConnected(profileId))
        {
            return Conflict(new
            {
                code = "ACTIVE_CONNECTION_CONFLICT",
                message = "Profile cannot be modified while an active WebSocket connection exists. Log out first."
            });
        }

        var profile = saveServer.GetProfile(profileId);
        if (profile == null)
        {
            return NotFound(new { message = $"Could not find profile with id {profileId}" });
        }

        var pmcData = profile?.CharacterData?.PmcData;
        if (pmcData == null)
        {
            return NotFound(new { message = "PmcData not found" });
        }

        var quest = pmcData?.Quests?
            .FirstOrDefault(q => q.QId == questId);
        if (quest == null)
        {
            return NotFound(new { message = $"Could not find quest with id {questId}" });
        }

        if (objectiveId != null)
        {
            var objective = pmcData!.TaskConditionCounters?
                .FirstOrDefault(tc => tc.Key == objectiveId)
                .Value;

            var templateQuest = templateTable.Quests
                    .FirstOrDefault(tq => tq.Key == questId)
                    .Value;
            if (templateQuest == null)
            {
                return NotFound(new { message = "Profile did not have any conditions for the quest and the template for the quest could not be found. This shouldn't be possible." });
            }

            var templateObjective = templateQuest.Conditions?.AvailableForFinish?
                    .FirstOrDefault(c => c.Id == objectiveId);
            if (templateObjective == null)
            {
                return NotFound(new { message = "Could not find condition in template quest" });
            }

            if (objective == null)
            {
                objective = new TaskConditionCounter
                {
                    Id = templateObjective.Id,
                    SourceId = questId,
                    Type = templateObjective.Type,
                    Value = templateObjective.Value
                };

                pmcData!.TaskConditionCounters?.Add(templateObjective.Id, objective);
            }
            else
            {
                objective.Value = templateObjective.Value;
            }

            (quest.CompletedConditions ??= []).Add(objectiveId);

            await saveServer.SaveProfileAsync(profileId);
            return Ok();
        }

        var response = questHelper.CompleteQuest(pmcData!, new CompleteQuestRequestData
        {
            QuestId = questId
        }, profileId);

        await saveServer.SaveProfileAsync(profileId);
        return Ok();
    }
}
