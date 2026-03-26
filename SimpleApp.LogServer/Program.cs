using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SimpleApp.LogServer.Features.Logs;
using SimpleApp.LogServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<LogDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        o => o.MigrationsAssembly(typeof(LogDbContext).Assembly.FullName)));

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Redirect root to the log dashboard UI
app.MapGet("/", () => Results.Redirect("/index.html"));

// Health check endpoint used by Docker
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithTags("Health");

// Auto-migrate on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LogDbContext>();
    await db.Database.MigrateAsync();
}

app.MapLogEndpoints();

app.Run();
