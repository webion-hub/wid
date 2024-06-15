using Webion.AspNetCore;

namespace Webion.IIS.Daemon.Config;

public sealed class CorsConfig : IWebApplicationConfiguration
{
    public void Add(WebApplicationBuilder builder)
    {
        var origins = builder.Configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? [];
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowCredentials();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
                policy.WithOrigins(origins);
            });
        });
    }

    public void Use(WebApplication app)
    {
        app.UseCors();
    }
}