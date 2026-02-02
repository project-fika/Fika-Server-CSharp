using FikaServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FikaServer.Models;

[AttributeUsage(AttributeTargets.Class)]
public class RequireApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string _authHeaderName = "Authorization";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        var services = context.HttpContext.RequestServices;

        if (!request.Headers.TryGetValue(_authHeaderName, out var authHeader))
        {
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = "Unauthorized: Missing Authorization header"
            };
            return;
        }

        var token = authHeader.Count > 0 ? authHeader[0] : null;

        if (string.IsNullOrEmpty(token) ||
            !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = "Unauthorized: Invalid Authorization format"
            };
            return;
        }

        var extractedToken = token.AsSpan(7).Trim();

        var configService = services.GetRequiredService<ConfigService>();
        var apiKey = configService.Config.Server.ApiKey.AsSpan();

        // Span-based equality check — avoids string allocation
        if (!extractedToken.Equals(apiKey, StringComparison.Ordinal))
        {
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = "Forbidden: Invalid API Key"
            };
            return;
        }

        await Task.CompletedTask;
    }
}
