using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public class OpenAdminSettings(ConfigService configService, NotificationWebSocket notificationWebSocket) : IFikaCommand
    {
        public string Command
        {
            get
            {
                return "showadminsettings";
            }
        }

        public string CommandHelp
        {
            get
            {
                return "fika showadminsettings\n\nOpens the settings GUI if you are a registered admin.";
            }
        }

        public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            bool isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
            await notificationWebSocket.SendAsync(sessionId, new OpenAdminMenuNotification(isAdmin));
            return isAdmin ? "Opening admin menu." : "You are not an admin!";
        }
    }
}
