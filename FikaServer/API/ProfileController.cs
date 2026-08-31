using FikaServer.Models;
using FikaShared.Enums;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/profile")]
[RequireApiKey]
public sealed class ProfileController(SaveServer saveServer, TemplateTable templateTable) : ControllerBase
{
    [HttpGet("quests")]
    public ActionResult<List<List<ActiveQuestData>>> GetQuests([FromQuery] string? profileId)
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
                        return new ActiveObjectiveData(c.Id!, (int)c.Value!, (int)target, state);
                    })
;
                return new ActiveQuestData(q.QId, q.Status == QuestStatusEnum.Success, conditionsData);
            })
            .ToList();

        return Ok(quests);
    }
}
