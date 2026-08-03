using FikaServer.Models.Fika.Config;
using FikaServer.Services.Cache;
using FikaServer.Services.Headless;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace FikaServer.OnLoad;


[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad)]
public sealed class FikaPostLoad(FikaServerConfig fikaServerConfig, HeadlessProfileService headlessProfileService,
    FriendRequestsService friendRequestsService, PlayerRelationsService playerRelationsCacheService) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (fikaServerConfig.Headless.Profiles.Amount > 0)
        {
            await headlessProfileService.OnPostLoadAsync(cancellationToken);
        }

        await playerRelationsCacheService.OnPostLoad();
        friendRequestsService.OnPostLoad();
    }
}
