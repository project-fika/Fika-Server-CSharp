﻿using FikaServer.Models.Fika.Config;
using FikaServer.Servers;
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;

namespace FikaServer.OnLoad;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]

public class FikaPostLoad(ISptLogger<FikaPostLoad> logger, ConfigServer configServer, /*NatPunchServer natPunchServer,*/ ImageRouter imageRouter,
    HeadlessProfileService HeadlessProfileService, LocaleService localeService, PlayerRelationsService playerRelationsCacheService,
    FriendRequestsService friendRequestsService, ConfigService fikaConfig, NatPunchServer natPunchServer, WebhookService webhookService) : IOnLoad
{
    public async Task OnLoad()
    {
        FikaConfig config = fikaConfig.Config;

        if (config.NatPunchServer.Enable)
        {
            natPunchServer.Start();
        }

        if (config.Headless.Profiles.Amount > 0)
        {
            await HeadlessProfileService.OnPostLoadAsync();
        }

        await localeService.OnPostLoadAsync();
        BlacklistSpecialProfiles();
        await playerRelationsCacheService.OnPostLoad();
        friendRequestsService.OnPostLoad();

        if (config.Server.Webhook.Enabled)
        {
            await webhookService.VerifyWebhook();
        }

        if (config.Background.Enable)
        {
            string imagePath = "assets/images/launcher/bg.png";
            imageRouter.AddRoute("/files/launcher/bg", Path.Join(fikaConfig.ModPath, imagePath));
        }
    }

    private void BlacklistSpecialProfiles()
    {
        CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();
        HashSet<string> profileBlacklist = coreConfig.Features.CreateNewProfileTypesBlacklist;

        if (!fikaConfig.Config.Server.ShowDevProfile)
        {
            profileBlacklist.Add("SPT Developer");
        }

        if (!fikaConfig.Config.Server.ShowNonStandardProfile)
        {
            foreach (string profile in (string[])["Tournament", "SPT Easy start", "SPT Zero to hero"])
            {
                profileBlacklist.Add(profile);
            }
        }
    }
}
