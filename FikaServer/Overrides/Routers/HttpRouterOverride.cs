using Microsoft.Extensions.Primitives;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Routers;
using System.Reflection;

namespace FikaServer.Overrides.Routers;

/// <summary>
/// This override is essential for setups where the backendIp or NAT port mapping differs from the SPT server’s backend port.
/// Without it, SPT constructs an incorrect backend URL, causing connection issues (e.g., using 0.0.0.0 instead of the correct address).
/// </summary>
public class GetResponseOverride : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HttpRouter).GetMethod(nameof(HttpRouter.GetResponse))!;
    }

    [PatchPostfix]
    public async static ValueTask<string?> Postfix(ValueTask<string?> __result, HttpRequest req)
    {
        HttpServerHelper httpServerHelper = ServiceLocator.ServiceProvider.GetService<HttpServerHelper>() ?? throw new NullReferenceException("HttpServerHelper is null!");

        string? response = await __result;

        if (!StringValues.IsNullOrEmpty(req.Headers.Host))
        {
            string originalHost = httpServerHelper.BuildUrl();
            string requestHost = req.Headers.Host.ToString();

            response = response?.Replace(originalHost, requestHost);
        }

        return response;
    }
}
