using System.Text.RegularExpressions;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace FikaServer.ChatBot.Commands;

[Injectable]
public partial class RemoveFleaBan(ConfigService configService, MailSendService mailSendService,
    SaveServer saveServer, NotificationSendHelper sendHelper, FikaProfileService fikaProfileService) : IFikaCommand
{
    [GeneratedRegex("^fika removefleaban (\\w+)$")]
    private static partial Regex RemoveFleaBanCommandRegex();

    public string Command
    {
        get
        {
            return "removefleaban";
        }
    }

    public string CommandHelp
    {
        get
        {
            return $"fika {Command}\nUnbans a player from the flea\nExample: fika removefleaban Nickname";
        }
    }

    public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var value = request.DialogId;
        var isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
        if (!isAdmin)
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "You are not an admin!");
            return value;
        }

        var text = request.Text;
        if (!RemoveFleaBanCommandRegex().IsMatch(text))
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "Invalid use of the command.");
            return value;
        }

        var split = text.Split(' ');
        var nickname = split[2];
        var profile = fikaProfileService.GetProfileByNickname(nickname);
        if (!profile.HasProfileData())
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"Could not find profile '{nickname}'.");
            return value;
        }

        var bans = profile.CharacterData.PmcData.Info.Bans;
        if (bans == null || !bans.Any(b => b.BanType == BanType.RagFair))
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"'{nickname}' does not have any flea bans.");
            return value;
        }

        // create a new filtered collection without flea bans
        profile.CharacterData.PmcData.Info.Bans = bans.Where(b => b.BanType != BanType.RagFair);

        await saveServer.SaveProfileAsync(profile.ProfileInfo.ProfileId.Value);

        mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"'{nickname}' has been unbanned from the flea.");

        sendHelper.SendMessage(profile.ProfileInfo.ProfileId.GetValueOrDefault(), new RemoveBanNotification()
        {
            EventType = NotificationEventType.InGameUnBan,
            EventIdentifier = new(),
            BanType = BanType.RagFair
        });

        return value;
    }


}
