using FikaServer.Models.Fika;
using FikaServer.Models.Fika.SendItem;
using FikaServer.Overrides.Callbacks;
using FikaServer.Overrides.Routers;
using FikaServer.Overrides.Services;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.OnLoad;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public class FikaPreLoad(ISptLogger<FikaPreLoad> logger, ClientService clientService,
    PlayerRelationsService playerRelationsCacheService, FriendRequestsService friendRequestsService,
    JsonUtil jsonUtil) : IOnLoad
{
    private bool _overridesInjected = false;
    private readonly List<AbstractPatch> _abstractPatches =
    [
        new GetResponseOverride(),
        new GetFriendListOverride(),
        new ListInboxOverride(),
        new ListOutboxOverride(),
        new SendFriendRequestOverride(),
        new AcceptAllFriendRequestsOverride(),
        new AcceptFriendRequestOverride(),
        new DeclineFriendRequestOverride(),
        new CancelFriendRequestOverride(),
        new DeleteFriendOverride(),
        new IgnoreFriendOverride(),
        new UnIgnoreFriendOverride(),
        new SendMessageOverride(),
        new GetMiniProfilesOverride(),
        new GetFriendsOverride(),
        new StartLocalRaidOverride(),
        new EndLocalRaidOverride(),
    ];

    private void InjectOverrides()
    {
        if (_overridesInjected)
        {
            return;
        }

        try
        {
            foreach (AbstractPatch patch in _abstractPatches)
            {
                logger.Debug($"[Fika Server] Loading patch: {patch.GetType().Name}");
                patch.Enable();
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error applying patch: {ex.Message}");
            throw;
        }

        _overridesInjected = true;
    }

    public async Task OnLoad()
    {
        InjectOverrides();

        BaseInteractionRequestDataConverter.RegisterModDataHandler(FikaItemEventRouter.SENDTOPLAYER, jsonUtil.Deserialize<SendItemRequestData>);

        clientService.OnPreLoad();
        await playerRelationsCacheService.OnPreLoad();
        await friendRequestsService.OnPreLoad();
    }
}
