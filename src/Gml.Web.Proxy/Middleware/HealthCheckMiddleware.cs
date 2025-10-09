using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Model;

namespace Gml.Web.Proxy.Middleware;

public class HealthInfoMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProxyStateLookup _stateLookup;

    public HealthInfoMiddleware(RequestDelegate next, IProxyStateLookup stateLookup)
    {
        _next = next;
        _stateLookup = stateLookup;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (_stateLookup.TryGetCluster("backend", out var cluster))
        {
            var destination = cluster.Destinations.First().Value;

            if (destination.Health.Active != DestinationHealth.Healthy && path != "/wait")
            {
                if (path.StartsWithSegments("/_next"))
                {
                    await _next(context);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.Response.Headers.Location = "/wait";
                return;
            }

            if (destination.Health.Active == DestinationHealth.Healthy && path == "/wait")
            {
                context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.Response.Headers.Location = "/";
                return;
            }
        }

        await _next(context);
    }
}
