using FikaServer.Models.Fika.Config;
using FikaServer.Servers;
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaServer.Services.Headless;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;

namespace FikaServer.OnLoad;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Preload)]
public class FikaPreLoad(ISptLogger<FikaPreLoad> logger, IEnumerable<IRuntimePatch> patches, ClientService clientService,
    PlayerRelationsService playerRelationsCacheService, FriendRequestsService friendRequestsService,
    HeadlessProfileService headlessProfileService, LocaleService localeService, NatPunchServer natPunchServer,
    WebhookService webhookService, ImageRouter imageRouter, FikaPaths fikaPaths,
    FikaServerConfig fikaServerConfig, CoreConfig coreConfig, HttpConfig httpConfig) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var patch in patches)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"[Fika Server] Loading patch: {patch.GetType().Name}");
                }

                patch.Enable();
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error applying patch: {ex.Message}");
            throw;
        }

        ApplySPTConfig(fikaServerConfig.Server.SPT);

        //BaseInteractionRequestDataConverter.RegisterModDataHandler(FikaItemEventRouter.SENDTOPLAYER, jsonUtil.Deserialize<SendItemRequestData>);

        clientService.OnPreLoad();
        await playerRelationsCacheService.OnPreLoad();
        await friendRequestsService.OnPreLoad();

        if (fikaServerConfig.NatPunchServer.Enable)
        {
            natPunchServer.Start();
        }

        if (fikaServerConfig.Headless.Profiles.Amount > 0)
        {
            await headlessProfileService.OnPostLoadAsync();
        }

        await localeService.OnPostLoadAsync();
        BlacklistSpecialProfiles();
        await playerRelationsCacheService.OnPostLoad();
        friendRequestsService.OnPostLoad();

        if (fikaServerConfig.Server.Webhook.Enabled)
        {
            await webhookService.VerifyWebhook();
        }

        if (fikaServerConfig.Background.Enable)
        {
            var imagePath = "assets/images/launcher/bg.png";
            imageRouter.AddRoute("/files/launcher/bg", Path.Join(fikaPaths.ModPath, imagePath));
        }
    }

    private void ApplySPTConfig(FikaSPTServerConfig config)
    {
        logger.Info("[Fika Server] Overriding SPT configuration");

        if (config.DisableSPTChatBots)
        {
            string commandoId = coreConfig.Features.ChatbotFeatures.Ids["commando"];
            string sptFriendId = coreConfig.Features.ChatbotFeatures.Ids["spt"];

            coreConfig.Features.ChatbotFeatures.EnabledBots[commandoId] = false;
            coreConfig.Features.ChatbotFeatures.EnabledBots[sptFriendId] = false;
        }

        httpConfig.Ip = config.Http.Ip;
        httpConfig.Port = config.Http.Port;
        httpConfig.BackendIp = config.Http.BackendIp;
        httpConfig.BackendPort = config.Http.BackendPort;
    }

    private void BlacklistSpecialProfiles()
    {
        var profileBlacklist = coreConfig.Features.CreateNewProfileTypesBlacklist;

        if (!fikaServerConfig.Server.ShowDevProfile)
        {
            profileBlacklist.Add("SPT Developer");
        }

        if (!fikaServerConfig.Server.ShowNonStandardProfile)
        {
            foreach (var profile in (string[])["Tournament", "SPT Easy start", "SPT Zero to hero"])
            {
                profileBlacklist.Add(profile);
            }
        }
    }
}
