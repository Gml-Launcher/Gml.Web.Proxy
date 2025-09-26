using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gml.Web.Proxy.Middleware;

public class MntRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public MntRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, GmlWebClientStateManager stateManager)
    {
        var path = context.Request.Path;

        var isInstalled = await stateManager.CheckInstalled();

        // if not installed - redirect to install
        if (path.HasValue && path.Equals("/") && !isInstalled)
        {
            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers.Location = "/mnt";
            return;
        }

        // if installed - redirect from install
        if (path.HasValue && path.StartsWithSegments("/mnt", out var _) && isInstalled)
        {
            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers.Location = "/";
            return;
        }

        await _next(context);
    }
}
