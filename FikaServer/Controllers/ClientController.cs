using FikaServer.Models.Fika.Config;
using FikaServer.Models.Fika.Routes.Client.Check;
using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FikaServer.Controllers
{
    [Injectable]
    public class ClientController(ClientService fikaClientService)
    {
        /// <summary>
        /// Handle /fika/client/config
        /// </summary>
        public string HandleClientConfig()
        {
            FikaConfigClient clientConfig = fikaClientService.GetClientConfig();

            // Here be dragons, this is technically not in the client config, or well it was.. But it was decided it was better for this configuration
            // To be together with 'sentItemsLoseFIR' so users could find both options easier.
            // Keep this here as this is really only supposed to be a 'client' config and it's really only used on the client.
            string config = JsonSerializer.Serialize(clientConfig);
            JsonNode configObject = JsonNode.Parse(config)!.AsObject();
            configObject["allowItemSending"] = fikaClientService.GetIsItemSendingAllowed();

            return configObject.ToJsonString();
        }

        /// <summary>
        /// Handle /fika/natpunchserver/config
        /// </summary>
        public FikaConfigNatPunchServer HandleNatPunchServerConfig()
        {
            return fikaClientService.GetNatPunchServerConfig();
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public FikaCheckModResponse HandleCheckMods(FikaCheckModRequestData request)
        {
            return fikaClientService.GetCheckModsResponse(request);
        }

        /// <summary>
        /// Handle /fika/client/check/mods
        /// </summary>
        public SptProfile? HandleProfileDownload(MongoId sessionId)
        {
            return fikaClientService.GetProfileBySessionID(sessionId);
        }

        /// <summary>
        /// Handle /fika/client/check/version
        /// </summary>
        public VersionCheckResponse HandleVersionCheck()
        {
            return fikaClientService.GetVersion();
        }
    }
}
