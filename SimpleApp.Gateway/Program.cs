using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// ── YARP Reverse Proxy ──────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        // Forward the authenticated user's identity to downstream services
        // so simpleapp can read claims without re-validating the token
        transformBuilder.AddRequestTransform(async ctx =>
        {
            if (ctx.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = ctx.HttpContext.User.FindFirst("sub")?.Value
                          ?? ctx.HttpContext.User.FindFirst("oid")?.Value;

                if (userId is not null)
                    ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
            }
            await Task.CompletedTask;
        });
    });

// ── JWT Auth (Azure AD B2C) ─────────────────────────────────────
// Gateway validates the token ONCE — downstream services trust X-User-Id header.
var azureAdB2C = builder.Configuration.GetSection("AzureAdB2C");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{azureAdB2C["Instance"]}/{azureAdB2C["Domain"]}/{azureAdB2C["SignUpSignInPolicyId"]}/v2.0";
        options.Audience = azureAdB2C["ClientId"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting ───────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // 429 Too Many Requests when limit is exceeded
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 100;
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

// ── Gateway Request Logging → LogServer ────────────────────────
builder.Services.AddHttpClient("logserver", client =>
{
    var logServerUrl = builder.Configuration["LogServer:Url"]!;
    client.BaseAddress = new Uri(logServerUrl);
    client.Timeout = TimeSpan.FromSeconds(2); // never slow down the gateway
});

var app = builder.Build();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ── Middleware: log every proxied request to LogServer ──────────
app.Use(async (ctx, next) =>
{
    var start = DateTimeOffset.UtcNow;
    await next();
    var elapsed = DateTimeOffset.UtcNow - start;

    // Fire-and-forget — never block the response
    _ = Task.Run(async () =>
    {
        try
        {
            var factory = ctx.RequestServices.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("logserver");
            var userId = ctx.User.FindFirst("sub")?.Value
                      ?? ctx.User.FindFirst("oid")?.Value
                      ?? "anonymous";

            var payload = new
            {
                message = $"{ctx.Request.Method} {ctx.Request.Path} → {ctx.Response.StatusCode}",
                level = ctx.Response.StatusCode >= 500 ? "ERROR"
                      : ctx.Response.StatusCode >= 400 ? "WARN"
                      : "INFO",
                service = "gateway",
                source = "RequestLoggingMiddleware",
                hostname = Environment.MachineName,
                attributes = new Dictionary<string, object>
                {
                    ["method"]     = ctx.Request.Method,
                    ["path"]       = ctx.Request.Path.ToString(),
                    ["statusCode"] = ctx.Response.StatusCode,
                    ["elapsedMs"]  = elapsed.TotalMilliseconds,
                    ["userId"]     = userId
                }
            };

            await client.PostAsJsonAsync("/api/logs", payload);
        }
        catch
        {
            // Swallow — logging must never affect the request pipeline
        }
    });
});

// Health check — no auth required
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "gateway" }))
    .AllowAnonymous();

app.MapReverseProxy();

app.Run();
