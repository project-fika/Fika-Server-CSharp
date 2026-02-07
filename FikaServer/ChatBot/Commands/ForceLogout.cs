using System.Text.RegularExpressions;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Servers.Ws;
using SPTarkov.Server.Core.Services;

namespace FikaServer.ChatBot.Commands;

[Injectable]
public partial class ForceLogout(ConfigService configService, MailSendService mailSendService,
    NotificationSendHelper sendHelper, SptWebSocketConnectionHandler websocketHandler,
    FikaProfileService fikaProfileService) : IFikaCommand
{
    [GeneratedRegex("^fika forcelogout (\\w+)$")]
    private static partial Regex ForceLogoutCommandRegex();

    public string Command
    {
        get
        {
            return "forcelogout";
        }
    }

    public string CommandHelp
    {
        get
        {
            return $"fika {Command}\nForces a client to logout if they are in the menu\nExample: fika forcelogout Nickname\nUse all to logout everyone (including you).";
        }
    }

    public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var value = request.DialogId;
        var isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
        if (!isAdmin)
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "You are not an admin!");
            return new(value);
        }

        var text = request.Text;
        if (!ForceLogoutCommandRegex().IsMatch(text))
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "Invalid use of the command.");
            return new(value);
        }

        var split = text.Split(' ');
        var nickname = split[2];

        if (nickname == "all")
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            "Everyone has been forced to log out.");
            websocketHandler.SendMessageToAll(new WsNotificationEvent()
            {
                EventType = NotificationEventType.ForceLogout,
                EventIdentifier = new()
            });

            return new(value);
        }

        mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"'{nickname}' has been forced to log out.");

        var profile = fikaProfileService.GetProfileByNickname(nickname)
            ?? throw new NullReferenceException($"Could not find profile {nickname}");
        sendHelper.SendMessage(profile.ProfileInfo.ProfileId.GetValueOrDefault(), new WsNotificationEvent()
        {
            EventType = NotificationEventType.ForceLogout,
            EventIdentifier = new()
        });

        return new(value);
    }


}
