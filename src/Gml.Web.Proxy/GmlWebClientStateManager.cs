using Yarp.ReverseProxy.Configuration;

namespace Gml.Web.Proxy;

public class GmlWebClientStateManager
{
    private bool? _isInstalledCache = null;
    private readonly IProxyConfigProvider _proxyConfigProvider;

    public GmlWebClientStateManager(IProxyConfigProvider proxyConfigProvider)
    {
        _proxyConfigProvider = proxyConfigProvider;
    }

    public async Task<bool> CheckInstalled()
    {
        try
        {
            if (_isInstalledCache is not null)
            {
                return _isInstalledCache ?? false;
            }

            var backend = _proxyConfigProvider.GetConfig();

            var cluster = backend.Clusters.First(c => c.ClusterId == "backend");

            using var client = new HttpClient();
            client.BaseAddress = new Uri(cluster.Destinations!["backend/d1"].Address);

            var response = await client.GetAsync("/api/v1/settings/checkInstalled");

            if (!response.IsSuccessStatusCode)
            {
                _isInstalledCache = !response.IsSuccessStatusCode;
            }

            return false;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return false;
        }
    }
}
