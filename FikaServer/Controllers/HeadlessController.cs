using FikaServer.Helpers;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Services;
using SPTarkov.DI.Annotations;

namespace FikaServer.Controllers;

[Injectable]
public class HeadlessController(HeadlessHelper headlessHelper, ConfigService fikaConfig)
{
    /// <summary>
    /// Handle /fika/headless/get
    /// </summary>
    /// <returns></returns>
    public GetHeadlessesResponse HandleGetHeadlesses()
    {
        return new GetHeadlessesResponse
        {
            Headlesses = headlessHelper.HeadlessClients.ToDictionary()
        };
    }

    /// <summary>
    /// Handle /fika/headless/available
    /// </summary>
    /// <returns></returns>
    public HeadlessAvailableClients[] HandleGetAvailableHeadlesses()
    {
        return headlessHelper.GetAvailableHeadlessClients();
    }

    /// <summary>
    /// Handle /fika/headless/restartafterraidamount
    /// </summary>
    /// <returns></returns>
    public GetHeadlessRestartAfterAmountOfRaids HandleRestartAfterRaidAmount()
    {
        return new GetHeadlessRestartAfterAmountOfRaids
        {
            Amount = fikaConfig.Config.Headless.RestartAfterAmountOfRaids
        };
    }
}
