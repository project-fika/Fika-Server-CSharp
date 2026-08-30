using FikaServer.Models;
using FikaShared.Responses;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/data")]
[RequireApiKey]
public sealed class GetDataController(TemplateTable templateTable, LocaleService localeService) : ControllerBase
{
    private readonly static HashSet<MongoId> _ignoredItems =
    [
        new("5e85aac65505fa48730d8af2"),
        new("62811d61578c54356d6d67ea"),
        new("628120415631d45211793c99"),
        new("628120f210e26c1f344e6558"),
        new("6281214c1d5df4475f46a33a"),
        new("6281215b4fa03b6b6c35dc6c"),
        new("628121651d5df4475f46a33c"),
        new("5ede47641cf3836a88318df1")
    ];

    [HttpGet]
    public ActionResult<GetDataResponse> HandleRequest()
    {
        var locale = localeService.GetLocaleDb("en");
        var handbookItems = templateTable.Handbook.Items
            .Where(x => x.Price != 0);

        var items = new Dictionary<string, ItemData>();
        foreach ((var itemId, var item) in templateTable.Items)
        {
            if (_ignoredItems.Contains(itemId))
            {
                continue;
            }

            if (item.IsQuestItem())
            {
                continue;
            }

            if (!handbookItems.Any(i => i.Id == itemId))
            {
                continue;
            }

            if (!locale.TryGetValue($"{itemId} Name", out var fullName) || string.IsNullOrWhiteSpace(fullName))
            {
                continue;
            }

            var description = locale.TryGetValue($"{itemId} Description", out var desc) ? desc : "Missing description";
            var stackAmount = item.Properties?.StackMaxSize ?? 1;
            var maxSendAmount = stackAmount * 10;

            items[itemId] = new ItemData
            {
                Name = fullName,
                Description = description,
                StackAmount = maxSendAmount
            };
        }

        var templateQuests = templateTable.Quests;
        var quests = new Dictionary<string, QuestData>(templateQuests.Count);
        foreach ((var questId, var quest) in templateQuests)
        {
            if (!locale.TryGetValue($"{questId} name", out var questName) || string.IsNullOrWhiteSpace(questName))
            {
                continue;
            }

            if (!locale.TryGetValue($"{questId} description", out var questDescription))
            {
                questDescription = string.Empty;
            }

            var hasConditions = quest.Conditions.AvailableForFinish?.Count > 0;
            var objectives = new List<QuestObjective>(hasConditions ? quest.Conditions.AvailableForFinish!.Count : 0);
            if (hasConditions)
            {
                foreach (var condition in quest.Conditions.AvailableForFinish!)
                {
                    if (!locale.TryGetValue(condition.Id, out var conditionDescription))
                    {
                        continue;
                    }

                    string description;
                    if (condition.Value != null && condition.Value > 1d)
                    {
                        description = $"{conditionDescription} 0/{condition.Value}";
                    }
                    else
                    {
                        description = conditionDescription;
                    }

                    objectives.Add(new QuestObjective(description));
                }
            }

            quests.Add(questId, new QuestData
            {
                Name = questName,
                Description = questDescription,
                Objectives = objectives
            });
        }

        GetDataResponse response = new()
        {
            Items = items,
            Quests = quests
        };

        return Ok(response);
    }
}
