using FikaServer.Services;
using FikaServer.Services.Headless;
using FikaShared;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using System.Net.WebSockets;
using System.Text;
using static FikaShared.Enums;

namespace FikaServer.Http.Get;

[Injectable(TypePriority = 0)]
public class HttpGetHeadless(ConfigService configService, HeadlessService headlessService, HttpResponseUtil httpResponseUtil) : BaseHttpRequest(configService)
{
    public override string Path { get; set; } = "/get/headless";

    public override string Method
    {
        get
        {
            return HttpMethods.Get;
        }
    }

    public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
    {
        var headlessClients = headlessService.HeadlessClients;
        var clients = new List<OnlineHeadless>(headlessClients.Count);
        foreach ((var profileId, var headlessClient) in headlessClients)
        {
            EHeadlessState state = headlessClient.WebSocket.State is WebSocketState.Open ? EHeadlessState.Ready : EHeadlessState.NotReady;
            clients.Add(new()
            {
                ProfileId = profileId,
                State = state,
                Players = headlessClient.Players != null ? headlessClient.Players.Count : 0
            });
        }

        var headlessResponse = new GetHeadlessResponse()
        {
            HeadlessClients = clients
        };

        resp.StatusCode = 200;
        resp.ContentType = "application/json";
        await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(headlessResponse)));
        await resp.StartAsync();
        await resp.CompleteAsync();
    }
}
