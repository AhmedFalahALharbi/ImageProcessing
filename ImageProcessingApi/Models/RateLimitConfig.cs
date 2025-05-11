namespace ImageProcessingApi.Models
{
    public class RateLimitConfig
    {
        public int RequestLimit { get; set; } = 100;
        public int TimeWindowInMinutes { get; set; } = 60;
    }
}