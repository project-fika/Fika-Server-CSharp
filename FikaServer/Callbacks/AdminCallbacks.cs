using FikaServer.Controllers;
using FikaServer.Models.Fika.Routes.Admin;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Callbacks
{
    [Injectable]
    public class AdminCallbacks(HttpResponseUtil httpResponseUtil, AdminController adminController)
    {

        public ValueTask<string> HandleGetSettings()
        {
            return new(httpResponseUtil.NoBody(adminController.HandleGetSettings()));
        }

        public ValueTask<string> HandleSetSettings(AdminSetSettingsRequest adminSetSettingsRequest, MongoId sessionId)
        {
            return new(httpResponseUtil.NoBody(adminController.HandleSetSettings(adminSetSettingsRequest, sessionId)));
        }
    }
}
