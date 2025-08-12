using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Raid;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks;

[Injectable]
public class HeadlessCallbacks(HttpResponseUtil httpResponseUtil, HeadlessController headlessController)
{
    /// <summary>
    /// Handle /fika/headless/get
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ValueTask<string> HandleGetHeadlesses(string url, FikaRaidServerIdRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(headlessController.HandleGetHeadlesses()));
    }

    /// <summary>
    /// Handle /fika/headless/available
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ValueTask<string> HandleAvailableHeadlesses(string url, FikaRaidServerIdRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(headlessController.HandleGetAvailableHeadlesses()));
    }

    /// <summary>
    /// Handle /fika/headless/restartafterraidamount
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ValueTask<string> HandleRestartAfterRaidAmount(string url, FikaRaidServerIdRequestData info, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(headlessController.HandleRestartAfterRaidAmount()));
    }
}
