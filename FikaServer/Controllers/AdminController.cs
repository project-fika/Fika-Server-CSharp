using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Admin;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Controllers;

[Injectable]
public class AdminController(ConfigService configService, ISptLogger<AdminController> logger)
{
    /// <summary>
    /// Handle /fika/admin/get
    /// </summary>
    /// <returns></returns>
    public AdminGetSettingsResponse HandleGetSettings()
    {
        return new(configService);
    }

    /// <summary>
    /// Handle /fika/admin/set
    /// </summary>
    /// <param name="adminSetSettingsRequest"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async ValueTask<AdminSetSettingsResponse> HandleSetSettings(AdminSetSettingsRequest adminSetSettingsRequest, MongoId sessionId)
    {
        if (!configService.Config.Server.AdminIds.Contains(sessionId))
        {
            logger.Warning($"{sessionId} tried updating the settings but is not an admin!");
            return new(false);
        }

        FikaConfigClient client = configService.Config.Client;

        client.FriendlyFire = adminSetSettingsRequest.FriendlyFire;
        client.AllowFreeCam = adminSetSettingsRequest.FreeCam;
        client.AllowSpectateFreeCam = adminSetSettingsRequest.SpectateFreeCam;
        client.SharedQuestProgression = adminSetSettingsRequest.SharedQuestProgression;
        configService.Config.Headless.SetLevelToAverageOfLobby = adminSetSettingsRequest.AverageLevel;

        logger.Info($"{sessionId} has updated the server settings");
        await configService.SaveConfig();
        return new(true);
    }
}
