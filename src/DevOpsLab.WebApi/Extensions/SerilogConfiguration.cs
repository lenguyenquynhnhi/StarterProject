using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace DevOpsLab.WebApi.Extensions;

/// <summary>
/// Structured logging in JSON from day one. Session 12 extends this with a
/// correlation id and richer request context — do not hand-roll string logs.
/// </summary>
public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service", "devopslab-webapi")
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .WriteTo.Console(new CompactJsonFormatter()));
    }
}
