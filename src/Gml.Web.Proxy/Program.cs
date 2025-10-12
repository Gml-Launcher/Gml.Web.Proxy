using Gml.Web.Proxy;
using Gml.Web.Proxy.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Swagger is optional; keep for dev diagnostics.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<GmlWebClientStateManager>();

// add memory cache
builder.Services.AddMemoryCache();

// Add YARP Reverse Proxy from configuration section "ReverseProxy"
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// app.UseHttpsRedirection();

// Map reverse proxy to handle incoming requests according to config
app.MapReverseProxy(builder =>
{
    // Custom middleware: return health info
    builder.UseMiddleware<HealthInfoMiddleware>();

    // Custom middleware: redirect /mnt to frontend when installed
    builder.UseMiddleware<MntRedirectMiddleware>();
});


app.Run();
