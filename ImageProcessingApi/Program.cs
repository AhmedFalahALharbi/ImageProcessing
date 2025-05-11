using ImageProcessingApi.Middleware;
using ImageProcessingApi.Services;
using ImageProcessingApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Configure rate limiting from appsettings.json
builder.Services.Configure<RateLimitConfig>(
    builder.Configuration.GetSection("RateLimitConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllers();

app.Run();