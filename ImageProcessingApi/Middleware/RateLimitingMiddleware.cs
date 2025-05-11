using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ImageProcessingApi.Models;

namespace ImageProcessingApi.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitConfig _config;
        private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitTracker = new();

        public RateLimitingMiddleware(RequestDelegate next, IOptions<RateLimitConfig> config)
        {
            _next = next;
            _config = config.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for the API key generation endpoint
            if (context.Request.Path.StartsWithSegments("/api/apikeys/generate"))
            {
                await _next(context);
                return;
            }

            // Get API key from context items (set by authentication middleware)
            if (!context.Items.TryGetValue("ApiKey", out var apiKeyObj) || apiKeyObj is not string apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key not authenticated");
                return;
            }

            // Track request count for this API key
            var rateLimitInfo = _rateLimitTracker.GetOrAdd(apiKey, _ => new RateLimitInfo());

            // Check if we need to reset the counter (time window has passed)
            if ((DateTime.UtcNow - rateLimitInfo.WindowStart).TotalMinutes >= _config.TimeWindowInMinutes)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.WindowStart = DateTime.UtcNow;
            }

            // Check if rate limit is exceeded
            if (rateLimitInfo.RequestCount >= _config.RequestLimit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            // Increment request count
            rateLimitInfo.RequestCount++;

            // Add rate limit headers
            context.Response.Headers.Add("X-RateLimit-Limit", _config.RequestLimit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", 
                (_config.RequestLimit - rateLimitInfo.RequestCount).ToString());

            await _next(context);
        }
    }

    public class RateLimitInfo
    {
        public int RequestCount { get; set; } = 0;
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }
}