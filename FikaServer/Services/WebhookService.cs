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
        var webhookUrl = configService.Config.Server.Webhook.Url;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.Error("Webhook URL is null or empty.");
            return false;
        }

        Uri url;
        try
        {
            url = new Uri(webhookUrl);
        }
        catch (UriFormatException ex)
        {
            logger.Error($"Unable to parse url: {webhookUrl}", ex);
            return false;
        }

        var message = new DiscordWebhook("Fika Server", "", "Server starting");
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.PutAsJsonAsync(url, message).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.Error($"HTTP request failed for webhook URL: {webhookUrl}", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.Error($"HTTP request timed out for webhook URL: {webhookUrl}", ex);
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error sending webhook to URL: {webhookUrl}", ex);
        }
        finally
        {
            response?.Dispose();
        }

        return false;
    }
}
