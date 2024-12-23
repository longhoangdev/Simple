using Microsoft.EntityFrameworkCore;
using SimpleApp;
using SimpleApp.Api.Base.Extensions;
using SimpleApp.Extensions;
using SimpleApp.Persistence.Contexts;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

app.MapEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    await using var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
}

app.Run();

