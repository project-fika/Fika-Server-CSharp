using FikaServer.Models.Fika.SendItem;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Controllers;

[Injectable]
public class SendItemController(ISptLogger<SendItemController> logger, EventOutputHolder eventOutputHolder,
    MailSendService mailSendService, InventoryHelper inventoryHelper, SaveServer saveServer, ItemHelper itemHelper,
    HttpResponseUtil httpResponseUtil, ConfigService fikaConfigService, NotificationWebSocket notificationWebSocket)
{
    public async ValueTask<ItemEventRouterResponse> SendItem(PmcData pmcData, SendItemRequestData body, string sessionId)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        if (body is null || body.ID is null || body.Target is null)
        {
            return httpResponseUtil.AppendErrorToOutput(output, "Missing data in body");
        }

        var senderProfile = saveServer.GetProfile(sessionId);

        if (!saveServer.ProfileExists(body.Target))
        {
            return httpResponseUtil.AppendErrorToOutput(output, "Target profile not found");
        }

        logger.Info($"{body.ID} is going to sessionID: {body.Target}");

        var senderItems = senderProfile.CharacterData?.PmcData?.Inventory?.Items ?? [];
        var itemsToSend = senderItems.GetItemWithChildren(body.ID);

        if (itemsToSend.Count == 0)
        {
            return httpResponseUtil.AppendErrorToOutput(output, "Item not found in inventory");
        }

        if (fikaConfigService.Config.Server.SentItemsLoseFIR)
        {
            foreach (var item in itemsToSend)
            {
                item.Upd ??= new();
                item.Upd.SpawnedInSession = false;
            }
        }

        mailSendService.SendSystemMessageToPlayer(body.Target,
            $"You have received a gift from {senderProfile?.CharacterData?.PmcData?.Info?.Nickname ?? "Unknown"}",
            itemsToSend, fikaConfigService.Config.Server.ItemSendingStorageTime * 86400); // days * seconds per day
        inventoryHelper.RemoveItem(senderProfile.CharacterData.PmcData, body.ID, sessionId, output);

        await notificationWebSocket.SendAsync(body.Target, new ReceivedSentItemNotification
        {
            Nickname = senderProfile?.CharacterData?.PmcData?.Info?.Nickname ?? "Unknown",
            TargetId = body.Target,
            ItemName = $"{itemsToSend[0].Template} ShortName"
        });

        return output;
    }

    public Dictionary<string, string> HandleAvailableReceivers(string sessionID)
    {
        Dictionary<string, string> result = [];

        if (!saveServer.ProfileExists(sessionID))
        {
            return result;
        }

        var sender = saveServer.GetProfile(sessionID);

        foreach (var profileKvP in saveServer.GetProfiles())
        {
            var profile = profileKvP.Value;

            // Skip freshly created profiles as they would cause an error
            if (profile.CharacterData?.PmcData?.Info is null)
            {
                continue;
            }

            // Skip headless clients
            if (profile.IsHeadlessProfile())
            {
                continue;
            }

            var Nickname = profile.CharacterData.PmcData?.Info?.Nickname;

            // Skip if the same user already exists in the results
            if (result.ContainsKey(Nickname))
            {
                continue;
            }

            // Skip if the profile is the sender himself
            if (Nickname == sender.CharacterData.PmcData.Info.Nickname)
            {
                continue;
            }

            result.Add(Nickname, profile.ProfileInfo.ProfileId);
        }

        return result;
    }
}
