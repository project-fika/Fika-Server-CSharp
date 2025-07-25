using FikaServer.Services;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetItems(DatabaseService databaseService, SPTarkov.Server.Core.Services.LocaleService localeService,
        HttpResponseUtil httpResponseUtil, ConfigService configService) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/items";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            var allItems = databaseService.GetItems();
            var locale = localeService.GetLocaleDb("en");

            var items = new Dictionary<string, ItemData>();
            foreach ((var itemId, var item) in allItems)
            {
                if (!locale.TryGetValue($"{itemId} Name", out var fullName))
                {
                    continue;
                }

                var description = locale.TryGetValue($"{itemId} Description", out var desc) ? desc : "Missing description";
                var stackAmount = item.Properties?.StackMaxSize ?? 1;
                var maxSendAmount = stackAmount * 10;

                var itemData = new ItemData
                {
                    Name = fullName,
                    Description = description,
                    StackAmount = maxSendAmount
                };

                items[itemId] = itemData;
            }

            GetItemsResponse response = new()
            {
                Items = items
            };

            resp.StatusCode = 200;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(response)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
