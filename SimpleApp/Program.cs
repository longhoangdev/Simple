using Microsoft.EntityFrameworkCore;
using SimpleApp;
using SimpleApp.Api.Base.Extensions;
using SimpleApp.Persistence.Contexts;
using Scalar.AspNetCore;
using Serilog;
using SimpleApp.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId();
    });

    builder.AddConfiguration();
    builder.ConfigureAuth();
    builder.AddMediaR();
    builder.MemoryCache();
    builder.Services.AddOpenApi();
    builder.Services.AddDatabase(builder.Configuration);

    builder.Services.AddEndpoints();

    var app = builder.Build();
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseSerilogRequestLogging();

    app.MapEndpoints();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        await using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        await context.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

