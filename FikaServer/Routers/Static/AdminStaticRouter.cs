using FikaServer.Callbacks;
using FikaServer.Models.Fika.Routes.Admin;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Routers.Static;

[Injectable]
public class AdminStaticRouter(AdminCallbacks adminCallbacks, JsonUtil jsonUtil) : StaticRouter(jsonUtil, [
        new RouteAction<AdminSetSettingsRequest>(
            "/fika/admin/set",
            async (
                url,
                info,
                sessionId,
                output
            ) => await adminCallbacks.HandleSetSettings(info, sessionId)
            ),
        new RouteAction<EmptyRequestData>(
            "/fika/admin/get",
            async (
                url,
                info,
                sessionId,
                output
            ) => await adminCallbacks.HandleGetSettings()
            )
    ])
{
}
