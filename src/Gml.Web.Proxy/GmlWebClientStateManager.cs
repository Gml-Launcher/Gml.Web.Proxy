using Microsoft.Extensions.Caching.Memory;
using Yarp.ReverseProxy.Configuration;

namespace Gml.Web.Proxy;

public class GmlWebClientStateManager
{
    private readonly IMemoryCache _cache;
    private readonly IProxyConfigProvider _proxyConfigProvider;

    private const string CacheKey = "GmlWebClientStateManager:IsInstalled";

    public GmlWebClientStateManager(
        IProxyConfigProvider proxyConfigProvider,
        IMemoryCache cache)
    {
        _proxyConfigProvider = proxyConfigProvider;
        _cache = cache;
    }

    public async Task<bool> CheckInstalled()
    {
        try
        {
            if (_cache.TryGetValue(CacheKey, out bool isInstalled))
            {
                return isInstalled;
            }

            var backend = _proxyConfigProvider.GetConfig();
            var cluster = backend.Clusters.First(c => c.ClusterId == "backend");

            using var client = new HttpClient
            {
                BaseAddress = new Uri(cluster.Destinations!["backend/d1"].Address)
            };

            var response = await client.GetAsync("/api/v1/settings/checkInstalled");

            isInstalled = !response.IsSuccessStatusCode;

            _cache.Set(CacheKey, isInstalled, TimeSpan.FromSeconds(10));

            return isInstalled;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }
}
