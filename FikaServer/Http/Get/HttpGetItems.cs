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
            var items = databaseService.GetItems();
            var locale = localeService.GetLocaleDb("en");

            var dictionary = new Dictionary<string, string>();
            foreach (var itemId in items.Keys)
            {
                if (locale.TryGetValue($"{itemId} Name", out var fullName))
                {
                    dictionary.Add(itemId, fullName);
                }
            }

            ItemsResponse response = new()
            {
                Items = dictionary
            };

            resp.StatusCode = 200;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(response)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
