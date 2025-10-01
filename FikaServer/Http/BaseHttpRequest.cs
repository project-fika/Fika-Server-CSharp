using FikaServer.Services;
using Microsoft.Extensions.Primitives;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers.Http;

namespace FikaServer.Http;

public abstract class BaseHttpRequest(ConfigService configService) : IHttpListener
{
    /// <summary>
    /// The path, e.g. <c>"/get/profiles"</c>
    /// </summary>
    public abstract string Path { get; set; }
    /// <summary>
    /// The <see cref="HttpMethods"/>, e.g. <see cref="HttpMethods.Get"/>
    /// </summary>
    public abstract string Method { get; }

    public bool CanHandle(MongoId sessionId, HttpContext context)
    {
        if (context.Request.Method != Method)
        {
            return false;
        }

        if (!string.Equals(context.Request.Path.Value, Path, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Handle the request <br/>
    /// Do not forget to set the <see cref="HttpResponse.StatusCode"/>
    /// </summary>
    /// <param name="req"></param>
    /// <param name="resp"></param>
    /// <returns></returns>
    public abstract Task HandleRequest(HttpRequest req, HttpResponse resp);

    public async Task Handle(MongoId sessionId, HttpContext context)
    {
        if (IsAuth(context.Request))
        {
            await HandleRequest(context.Request, context.Response);
        }
        else
        {
            context.Response.StatusCode = 403;
            await context.Response.StartAsync();
            await context.Response.CompleteAsync();
        }
    }

    private bool IsAuth(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Auth", out StringValues authHeader))
        {
            return false;
        }

        return authHeader.Contains(configService.Config.Server.ApiKey);
    }

    public static class ContentTypes
    {
        public const string Json = "application/json";
        public const string PlainText = "text/plain";
    }
}
