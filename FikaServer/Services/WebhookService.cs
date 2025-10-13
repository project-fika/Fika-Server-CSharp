using FikaServer.Models.Fika.Config;
using FikaServer.Models.Webhook;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class WebhookService(ISptLogger<ConfigService> logger, ConfigService configService)
{
    private readonly HttpClient _httpClient = new();
    private bool _verified;

    private FikaWebhookConfig WebhookConfig
    {
        get
        {
            return configService.Config.Server.Webhook;
        }
    }

    public async Task<bool> VerifyWebhook()
    {
        if (!WebhookConfig.Enabled)
        {
            return false;
        }

        var webhookUrl = WebhookConfig.Url;
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

        var message = new DiscordWebhook(WebhookConfig.Name, WebhookConfig.AvatarURL, "Server started");
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.PostAsJsonAsync(url, message);
            response.EnsureSuccessStatusCode();
            _verified = true;
            logger.LogWithColor("Fika webhook verified", LogTextColor.Green);
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

    public async Task SendWebhookMessage(string message)
    {
        if (!WebhookConfig.Enabled)
        {
            return;
        }

        if (!_verified)
        {
            logger.Error("A webhook broadcast was attempted when it's not verified or disabled.");
            return;
        }

        var webhookMessage = new DiscordWebhook(WebhookConfig.Name, WebhookConfig.AvatarURL, message);
        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.PostAsJsonAsync(WebhookConfig.Url, webhookMessage);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.Error("HTTP request failed for webhook", ex);
        }
        catch (TaskCanceledException ex)
        {
            logger.Error("HTTP request timed out for webhook", ex);
        }
        catch (Exception ex)
        {
            logger.Error("Unexpected error sending webhook", ex);
        }
        finally
        {
            response?.Dispose();
        }
    }
}
