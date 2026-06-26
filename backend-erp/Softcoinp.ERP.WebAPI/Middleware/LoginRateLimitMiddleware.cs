using System.Collections.Concurrent;

namespace Softcoinp.ERP.WebAPI.Middleware;

public class LoginRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoginRateLimitMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, LoginAttempt> _attempts = new();

    private const int MaxAttempts = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public LoginRateLimitMiddleware(RequestDelegate next, ILogger<LoginRateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/login") &&
            context.Request.Method == "POST")
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            var entry = _attempts.GetOrAdd(ip, _ => new LoginAttempt());
            lock (entry)
            {
                if (entry.ResetAt < now)
                {
                    entry.Count = 0;
                    entry.ResetAt = now.Add(Window);
                }

                entry.Count++;

                if (entry.Count > MaxAttempts)
                {
                    _logger.LogWarning("Login rate limit exceeded for IP: {Ip}", ip);
                    context.Response.StatusCode = 429;
                    context.Response.ContentType = "application/json";
                    context.Response.WriteAsync("{\"message\":\"Demasiados intentos. Intente de nuevo mas tarde.\"}");
                    return;
                }
            }
        }

        await _next(context);
    }

    private class LoginAttempt
    {
        public int Count { get; set; }
        public DateTime ResetAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}
