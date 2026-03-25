using Microsoft.EntityFrameworkCore;
using SimpleApp.Persistence.Contexts;

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
    
    // public static void AddAppConfig(this IConfigurationBuilder builder)
    // {
    //     var applicationId = Environment.GetEnvironmentVariable("ApplicationId");
    //     var environmentId = Environment.GetEnvironmentVariable("EnvironmentId");
    //     var profileId = Environment.GetEnvironmentVariable("ProfileId");
    //     if (string.IsNullOrWhiteSpace(applicationId) || string.IsNullOrWhiteSpace(environmentId) ||
    //         string.IsNullOrWhiteSpace(profileId))
    //         return;
    //     builder.AddAppConfig(applicationId, environmentId, profileId);
    // }
}