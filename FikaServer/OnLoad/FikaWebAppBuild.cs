using FikaServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.External;

namespace FikaServer.OnLoad
{
    [Injectable]
    public class FikaWebAppBuild(ConfigService configService) : IOnWebAppBuildModAsync
    {
        public async Task OnWebAppBuildAsync()
        {
            await configService.OnWebAppBuildAsync();
        }
    }
}
