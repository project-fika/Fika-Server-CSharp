using Microsoft.Extensions.Primitives;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Routers;

namespace FikaServer.Overrides.Routers
{
    /// <summary>
    /// This override is essential for setups where the backendIp or NAT port mapping differs from the SPT server’s backend port.
    /// Without it, SPT constructs an incorrect backend URL, causing connection issues (e.g., using 0.0.0.0 instead of the correct address).
    /// </summary>
    [Injectable]
    public class HttpRouterOverride(HttpServerHelper httpServerHelper,
        IEnumerable<StaticRouter> staticRouters, 
        IEnumerable<DynamicRouter> dynamicRoutes) : HttpRouter(staticRouters, dynamicRoutes)
    {
        public async override ValueTask<string?> GetResponse(HttpRequest req, string sessionID, string? body)
        {
            var response = await base.GetResponse(req, sessionID, body);

            if (!StringValues.IsNullOrEmpty(req.Headers.Host))
            {
                string originalHost = httpServerHelper.BuildUrl();
                string requestHost = req.Headers.Host.ToString();

                response = response.Replace(originalHost, requestHost);
            }

            return response;
        }
    }
}
