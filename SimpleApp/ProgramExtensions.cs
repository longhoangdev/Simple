using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SimpleApp.Api.Base.Behaviors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using SimpleApp.Persistence.Contexts;
using SimpleApp.Shared.Constants;
using SimpleApp.Shared.Extensions;
using SimpleApp.Shared.Caching;

namespace SimpleApp;

internal static class ProgramExtensions
{
    public static void AddMediaR(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.RegisterServicesFromAssemblyContaining<ApplicationAssemblyResolver>();
            
            config.AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(CommandValidationBehavior<,>));
        });
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
    }

    internal static void ConfigureAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAdB2C", options);

            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.NameClaimType = GlobalConstants.Claims.Name;

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async ctx =>
                {
                    if (ctx.Principal is null)
                    {
                        ctx.Fail("Not possible to detect user context.");
                        return;
                    }

                    var userId = ctx.Principal.FindFirstValue(GlobalConstants.Claims.UserId);
                    if (userId.IsBlank())
                    {
                        ctx.Fail("Not possible to detect user_id.");
                        return;
                    }

                    var context = ctx.HttpContext.RequestServices.GetRequiredService<ReadOnlyDataContext>();

                    var claims = new List<Claim>
                    {
                        new(GlobalConstants.Claims.UserId, userId)
                    };

                    ctx.Principal.AddIdentity(new ClaimsIdentity(claims));
                }
            };
        }, options => { builder.Configuration.Bind("AzureAdB2C", options); });
    }

    internal static void MemoryCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<ICacheService, CacheService>();
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            var redisConnection = builder.Configuration.GetConnectionString("Redis");
            options.Configuration = redisConnection;
        });
        
        builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
    }
    
}