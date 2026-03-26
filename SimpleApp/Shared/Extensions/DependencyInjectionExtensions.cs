﻿﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Api.Features.Users;
using SimpleApp.Persistence.Contexts;
using SimpleApp.Shared.Logging;

namespace SimpleApp.Shared.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"), npgsqlOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                sqlOptions.MigrationsHistoryTable("__MigrationsHistory");
            });
        });

        services.AddScoped<ReadOnlyDataContext>();

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Single consumer handles all User domain events (Created, Updated, Deleted)
            x.AddConsumer<UserEventConsumer, UserEventConsumerDefinition>();

            // Transactional Outbox: events are stored in PostgreSQL within the same
            // DB transaction as SaveChangesAsync, guaranteeing no dual-write issues.
            // Idempotency is handled automatically via the InboxState table.
            x.AddEntityFrameworkOutbox<DataContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], configuration["RabbitMq:VirtualHost"], h =>
                {
                    h.Username(configuration["RabbitMq:Username"]!);
                    h.Password(configuration["RabbitMq:Password"]!);
                });

                // Auto-map consumers to queues using ConsumerDefinition endpoint names.
                // Dead-letter queues are created automatically as <queue-name>_error.
                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static IServiceCollection AddDataDogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IDataDogLogService, DataDogLogService>(client =>
        {
            var baseUrl = configuration["DataDog:LogsApiUrl"]!;
            var apiKey = configuration["DataDog:ApiKey"];

            client.BaseAddress = new Uri(baseUrl);

            // Only add API key header when using the real DataDog service
            if (!string.IsNullOrWhiteSpace(apiKey))
                client.DefaultRequestHeaders.Add("DD-API-KEY", apiKey);

            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}