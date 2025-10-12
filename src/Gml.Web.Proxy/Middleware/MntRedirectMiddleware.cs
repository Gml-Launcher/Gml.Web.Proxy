using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Model;

namespace Gml.Web.Proxy.Middleware;

public class MntRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _stateLookup;

    public MntRedirectMiddleware(RequestDelegate next, IProxyStateLookup stateLookup)
    {
        _next = next;
        _stateLookup = stateLookup;
    }

    public async Task InvokeAsync(HttpContext context, GmlWebClientStateManager stateManager)
    {
        var data = context.GetReverseProxyFeature();

        if (_stateLookup.TryGetCluster("backend", out var cluster))
        {
            var destination = cluster.Destinations.First().Value;

            if (destination.Health.Active == DestinationHealth.Healthy)
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
            }
        }

        await _next(context);
    }
}
