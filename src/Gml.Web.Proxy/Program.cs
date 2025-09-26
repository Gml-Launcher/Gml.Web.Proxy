using Gml.Web.Proxy;
using Gml.Web.Proxy.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Swagger is optional; keep for dev diagnostics.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<GmlWebClientStateManager>();

// Add YARP Reverse Proxy from configuration section "ReverseProxy"
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Custom middleware: redirect /mnt to frontend when installed
app.UseMiddleware<MntRedirectMiddleware>();

// Map reverse proxy to handle incoming requests according to config
app.MapReverseProxy();

app.Run();
