using FikaServer.Models.Webhook;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class WebhookService(ISptLogger<ConfigService> logger, ConfigService configService)
{
    private readonly HttpClient _httpClient = new();

    public async Task<bool> VerifyWebhook()
    {
        Uri? url = null;
        try
        {
            url = new Uri(configService.Config.Server.Webhook.Url);
        }
        catch (UriFormatException ex)
        {
            logger.Error($"Unable to parse url: {configService.Config.Server.Webhook.Url}", ex);
            return false;
        }

        var message = new DiscordWebhook("Fika Server", "", "Server starting");
        var response = await _httpClient.PutAsJsonAsync(url, message);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.Error($"Unable to send webhook", ex);
            return false;
        }

        return true;
    }
}
