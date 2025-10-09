using FikaServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FikaServer.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequireApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string AuthHeaderName = "Authorization";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            ConfigService configService = context.HttpContext.RequestServices.GetRequiredService<ConfigService>();
            HttpRequest request = context.HttpContext.Request;

            if (!request.Headers.TryGetValue(AuthHeaderName, out var authHeader))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Unauthorized: Missing Authorization header"
                };
                return;
            }

            string token = authHeader.ToString();

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "Unauthorized: Invalid Authorization format"
                };
                return;
            }

            string extractedToken = token.Substring(7).Trim();

            if (extractedToken != configService.Config.Server.ApiKey)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = "Forbidden: Invalid API Key"
                };
                return;
            }

            await Task.CompletedTask;
        }
    }
}
