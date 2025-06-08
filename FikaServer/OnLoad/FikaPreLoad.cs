using FikaServer.Models.Fika;
using FikaServer.Models.Fika.SendItem;
using FikaServer.Services;
using FikaServer.Services.Cache;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.OnLoad
{
    [Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
    public class FikaPreLoad(ConfigService fikaConfig, ClientService clientService,
        PlayerRelationsService playerRelationsCacheService, FriendRequestsService friendRequestsService,
        JsonUtil jsonUtil) : IOnLoad
    {
        public async Task OnLoad()
        {
            BaseInteractionRequestDataConverter.RegisterModDataHandler(FikaItemEventRouter.SENDTOPLAYER, jsonUtil.Deserialize<SendItemRequestData>);

            await fikaConfig.OnPreLoad();
            clientService.OnPreLoad();
            playerRelationsCacheService.OnPreLoad();
            friendRequestsService.OnPreLoad();
        }
    }
}
