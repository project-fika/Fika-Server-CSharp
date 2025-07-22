using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaShared.Requests;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System.Collections.Frozen;
using System.Text;

namespace FikaServer.Http.Post
{
    [Injectable(TypePriority = 0)]
    public class HttpSendItem(ConfigService configService, JsonUtil jsonUtil,
        MailSendService mailSendService, ItemFilterService itemFilterService,
        ItemHelper itemHelper, PresetHelper presetHelper, ICloner cloner) : BaseHttpRequest(configService)
    {
        protected static readonly FrozenSet<MongoId> _excludedPresetItems =
        [
            ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_RED,
            ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_GREEN,
            ItemTpl.FLARE_RSP30_REACTIVE_SIGNAL_CARTRIDGE_YELLOW,
        ];

        public override string Path { get; set; } = "post/senditem";

        public override string Method
        {
            get
            {
                return HttpMethods.Post;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {

            

            using (StreamReader sr = new(req.Body))
            {
                string rawData = await sr.ReadToEndAsync();

                SendItemRequest request = jsonUtil.Deserialize<SendItemRequest>(rawData);
                if (request != null)
                {
                    MongoId profileId = new(request.ProfileId);
                    MongoId itemTpl = new(request.ItemTemplate);
                    int quantity = request.Amount;
                    string message = request.Message;

                    var checkedItem = itemHelper.GetItem(itemTpl);
                    if (!checkedItem.Key)
                    {
                        resp.StatusCode = 400;
                        await resp.Body.WriteAsync(Encoding.UTF8.GetBytes("That item could not be found."));
                        await resp.StartAsync();
                        await resp.CompleteAsync();

                        return;
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
                                itemsToSend.Add(
                                    new Item
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
                                resp.StatusCode = 400;
                                await resp.Body.WriteAsync(Encoding.UTF8.GetBytes("Too many items requested. Please lower the amount and try again."));
                                await resp.StartAsync();
                                await resp.CompleteAsync();

                                return;
                            }
                        }
                    }

                    // Flag the items as FiR
                    itemHelper.SetFoundInRaid(itemsToSend);

                    mailSendService.SendSystemMessageToPlayer(
                        profileId,
                        message,
                        itemsToSend
                    );
                }
                else
                {
                    resp.StatusCode = 400;
                    await resp.StartAsync();
                    await resp.CompleteAsync();

                    return;
                }
            }

            resp.StatusCode = 200;
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
