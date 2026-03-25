namespace SimpleApp.Shared.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddConfiguration(this WebApplicationBuilder builder)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());
        configBuilder.AddJsonFile("appsettings.json");
        configBuilder.AddJsonFile($"appsettings.{environmentName}.json");

        configBuilder.AddEnvironmentVariables();
        
        builder.Configuration.AddConfiguration(configBuilder.Build());
    }
}