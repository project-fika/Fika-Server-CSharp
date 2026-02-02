using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;

namespace FikaServer.ChatBot.Commands;

[Injectable]
public class AdminSettings(ConfigService configService, NotificationWebSocket notificationWebSocket, MailSendService mailSendService) : IFikaCommand
{
    public string Command
    {
        get
        {
            return "adminsettings";
        }
    }

    public string CommandHelp
    {
        get
        {
            return $"fika {Command}\n\nOpens the settings GUI if you are a registered admin.";
        }
    }

    public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
        await notificationWebSocket.SendAsync(sessionId, new OpenAdminMenuNotification(isAdmin));

        mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            isAdmin ? "Opening admin menu." : "You are not an admin!");
        return new(request.DialogId);
    }
}
