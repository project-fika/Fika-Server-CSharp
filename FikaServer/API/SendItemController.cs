using System.Collections.Frozen;
using FikaServer.Models;
using FikaShared.Requests;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace FikaServer.API;

[ApiController]
[Route("fika/api/senditem")]
[RequireApiKey]
public class SendItemController(MailSendService mailSendService, ItemHelper itemHelper,
    PresetHelper presetHelper, ICloner cloner) : ControllerBase
{
    protected static readonly FrozenSet<MongoId> _excludedPresetItems =
    [
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_RED,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_GREEN,
        ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_YELLOW,
    ];

    [HttpPost]
    public IActionResult HandlePostRequest([FromBody] SendItemRequest request)
    {
        MongoId profileId = new(request.ProfileId);
        MongoId itemTpl = new(request.ItemTemplate);
        var quantity = request.Amount;
        var message = request.Message;

        var checkedItem = itemHelper.GetItem(itemTpl);
        if (!checkedItem.Key)
        {
            return BadRequest("That item could not be found.");
        }

        List<Item> itemsToSend = [];
        var preset = presetHelper.GetDefaultPreset(checkedItem.Value.Id);
        if (preset is not null && !_excludedPresetItems.Contains(checkedItem.Value.Id))
        {
            for (var i = 0; i < quantity; i++)
            {
                var items = cloner.Clone(preset.Items);
                items = items.ReplaceIDs().ToList();
                itemsToSend.AddRange(items);
            }
        }
        else if (itemHelper.IsOfBaseclass(checkedItem.Value.Id, BaseClasses.AMMO_BOX))
        {
            for (var i = 0; i < quantity; i++)
            {
                List<Item> ammoBoxArray =
                [
                    new() { Id = new MongoId(), Template = checkedItem.Value.Id },
                            // DO NOT generate the ammo box cartridges, the mail service does it for us! :)
                            // _itemHelper.addCartridgesToAmmoBox(ammoBoxArray, checkedItem[1]);
                        ];
                // DO NOT generate the ammo box cartridges, the mail service does it for us! :)
                // _itemHelper.addCartridgesToAmmoBox(ammoBoxArray, checkedItem[1]);
                itemsToSend.AddRange(ammoBoxArray);
            }
        }
        else
        {
            if (checkedItem.Value.Properties.StackMaxSize == 1)
            {
                for (var i = 0; i < quantity; i++)
                {
                    itemsToSend.Add(new Item
                    {
                        Id = new MongoId(),
                        Template = checkedItem.Value.Id,
                        Upd = itemHelper.GenerateUpdForItem(checkedItem.Value),
                    }
                    );
                }
            }
            else
            {
                var itemToSend = new Item
                {
                    Id = new MongoId(),
                    Template = checkedItem.Value.Id,
                    Upd = itemHelper.GenerateUpdForItem(checkedItem.Value),
                };
                itemToSend.Upd.StackObjectsCount = quantity;
                try
                {
                    itemsToSend.AddRange(itemHelper.SplitStack(itemToSend));
                }
                catch
                {
                    return BadRequest("Too many items requested. Please lower the amount and try again.");
                }
            }
        }

        if (request.FoundInRaid)
        {
            // Flag the items as FiR
            itemHelper.SetFoundInRaid(itemsToSend);
        }
        mailSendService.SendSystemMessageToPlayer(profileId, message, itemsToSend, request.ExpirationDays * 86400); // days * seconds per day

        return Ok();
    }
}