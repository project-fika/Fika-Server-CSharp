using FikaServer.Services;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetItems(DatabaseService databaseService, SPTarkov.Server.Core.Services.LocaleService localeService,
        HttpResponseUtil httpResponseUtil, ConfigService configService) : BaseHttpRequest(configService)
    {
        private readonly static HashSet<MongoId> _ignoredItems = [
            new("5e85aac65505fa48730d8af2"),
            new("62811d61578c54356d6d67ea"),
            new("628120415631d45211793c99"),
            new("628120f210e26c1f344e6558"),
            new("6281214c1d5df4475f46a33a"),
            new("6281215b4fa03b6b6c35dc6c"),
            new("628121651d5df4475f46a33c"),
            new("5ede47641cf3836a88318df1")
            ];

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
            var handbookItems = databaseService.GetHandbook().Items
                .Where(x => x.Price != 0);

            var items = new Dictionary<string, ItemData>();
            foreach ((var itemId, var item) in allItems)
            {
                if (_ignoredItems.Contains(itemId))
                {
                    continue;
                }

                if (item.IsQuestItem())
                {
                    continue;
                }

                if (!handbookItems.Any(i => i.Id == itemId))
                {
                    continue;
                }

                if (!locale.TryGetValue($"{itemId} Name", out var fullName) || string.IsNullOrWhiteSpace(fullName))
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
            resp.ContentType = ContentTypes.Json;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(response)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
