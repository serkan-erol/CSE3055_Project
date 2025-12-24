using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Kismet.API.Middleware;

/// <summary>
/// Middleware to read access token from HTTP-only cookie and add it to Authorization header
/// This allows existing code that reads from Authorization header to continue working
/// </summary>
public class CookieTokenMiddleware
{
    private readonly RequestDelegate _next;

    public CookieTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // If Authorization header is not present, try to get token from cookie
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var accessToken = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Add the token to the Authorization header
                context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            }
        }

        await _next(context);
    }
}

