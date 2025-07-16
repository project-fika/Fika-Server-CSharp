using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using System.Text;

namespace FikaServer.ChatBot.Commands
{
    [Injectable]
    public class ListProfiles(ConfigService configService, NotificationWebSocket notificationWebSocket,
        MailSendService mailSendService, SaveServer saveServer) : IFikaCommand
    {
        public string Command
        {
            get
            {
                return "listprofiles";
            }
        }

        public string CommandHelp
        {
            get
            {
                return $"fika {Command}\nLists all profileIds.";
            }
        }

        public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
        {
            bool isAdmin = configService.Config.Server.AdminIds.Contains(sessionId);
            if (!isAdmin)
            {
                mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                    "You are not an admin!");
                return request.DialogId;
            }

            Dictionary<MongoId, SptProfile> profiles = saveServer.GetProfiles();
            StringBuilder sb = new(profiles.Count);
            foreach ((MongoId id, SptProfile profile) in profiles)
            {
                sb.AppendLine($"{id} - {profile?.CharacterData?.PmcData?.Info?.MainProfileNickname ?? "Unknown"}");
            }
            
            await notificationWebSocket.SendAsync(sessionId, new OpenAdminMenuNotification(isAdmin));
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                $"All profiles:\n\n{sb}");
            return new(request.DialogId);
        }
    }
}
