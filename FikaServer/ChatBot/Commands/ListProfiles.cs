using FikaServer.Models.Fika.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Commerce;
using System.Text;

namespace FikaServer.ChatBot.Commands;

[Injectable]
public class ListProfiles(FikaServerConfig fikaServerConfig,
    SaveServer saveServer,
    MailSendService mailSendService) : IFikaCommand
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
            return $"fika {Command}\nLists all profileIds and nicknames.\nNicknames are used for most commands";
        }
    }

    public async ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var isAdmin = fikaServerConfig.Server.AdminIds.Contains(sessionId);
        if (!isAdmin)
        {
            mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
                "You are not an admin!");
            return request.DialogId;
        }

        var profiles = saveServer.GetProfiles().Values;
        StringBuilder sb = new(profiles.Count);
        foreach (var profile in profiles)
        {
            if (!profile.HasProfileData())
            {
                continue;
            }

            sb.AppendLine($"{profile.CharacterData.PmcData.Info.Nickname} - {profile.ProfileInfo.ProfileId.GetValueOrDefault()}");
        }

        mailSendService.SendUserMessageToPlayer(sessionId, commandHandler,
            $"All profiles:\n\n{sb}");
        return new(request.DialogId);
    }
}
