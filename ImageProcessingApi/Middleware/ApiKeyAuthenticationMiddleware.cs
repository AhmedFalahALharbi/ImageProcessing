using ImageProcessingApi.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ImageProcessingApi.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, IApiKeyService apiKeyService)
        {
            _next = next;
            _apiKeyService = apiKeyService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for API key generation endpoint
            if (context.Request.Path.StartsWithSegments("/api/apikeys/generate"))
            {
                await _next(context);
                return;
            }

            // Check for API key in headers
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            string apiKey = apiKeyValue.ToString();

            // Validate API key
            if (!_apiKeyService.IsValidApiKey(apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // Store API key in HttpContext items for later use in rate limiting
            context.Items["ApiKey"] = apiKey;

            await _next(context);
        }
    }
}