
namespace FikaWebApp.Services;

public class BackgroundInitializerService(HeartbeatService heartbeatService, ItemCacheService itemCacheService, SendTimersService sendTimersService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        heartbeatService.Start();
        await itemCacheService.PopulateDictionary();
        await sendTimersService.Load();
    }
}
