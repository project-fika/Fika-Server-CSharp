using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Admin;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Controllers;

[Injectable]
public class AdminController(FikaServerConfig fikaServerConfig, FikaPaths fikaPaths, ISptLogger<AdminController> logger)
{
    /// <summary>
    /// Handle /fika/admin/get
    /// </summary>
    /// <returns></returns>
    public AdminGetSettingsResponse HandleGetSettings()
    {
        return new(fikaServerConfig);
    }

    /// <summary>
    /// Handle /fika/admin/set
    /// </summary>
    /// <param name="adminSetSettingsRequest"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async ValueTask<AdminSetSettingsResponse> HandleSetSettings(AdminSetSettingsRequest adminSetSettingsRequest, MongoId sessionId)
    {
        if (!fikaServerConfig.Server.AdminIds.Contains(sessionId))
        {
            logger.Warning($"{sessionId} tried updating the settings but is not an admin!");
            return new(false);
        }

        var client = fikaServerConfig.Client;

        client.FriendlyFire = adminSetSettingsRequest.FriendlyFire;
        client.AllowFreeCam = adminSetSettingsRequest.FreeCam;
        client.AllowSpectateFreeCam = adminSetSettingsRequest.SpectateFreeCam;
        client.SharedQuestProgression = adminSetSettingsRequest.SharedQuestProgression;
        fikaServerConfig.Headless.SetLevelToAverageOfLobby = adminSetSettingsRequest.AverageLevel;

        logger.Info($"{sessionId} has updated the server settings");
        await FikaConfigFile.SaveAsync(fikaServerConfig, fikaPaths.ConfigFilePath);
        return new(true);
    }
}
