using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Admin;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks;

[Injectable]
public class AdminCallbacks(HttpResponseUtil httpResponseUtil, AdminController adminController)
{
    /// <summary>
    /// Handle /fika/admin/get
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> HandleGetSettings()
    {
        return new(httpResponseUtil.NoBody(adminController.HandleGetSettings()));
    }

    /// <summary>
    /// Handle /fika/admin/set
    /// </summary>
    /// <param name="adminSetSettingsRequest"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async ValueTask<string> HandleSetSettings(AdminSetSettingsRequest adminSetSettingsRequest, MongoId sessionId)
    {
        return new(httpResponseUtil.NoBody(await adminController.HandleSetSettings(adminSetSettingsRequest, sessionId)));
    }
}
