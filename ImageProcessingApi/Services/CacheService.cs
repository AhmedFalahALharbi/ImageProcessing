using System;
using System.Collections.Concurrent;

namespace ImageProcessingApi.Services
{
    public interface ICacheService
    {
        bool TryGetValue<T>(string key, out T value);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
    }

    public class InMemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public bool TryGetValue<T>(string key, out T value)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                // Check if the item has expired
                if (DateTime.UtcNow > cacheItem.ExpirationTime)
                {
                    // Remove expired item
                    _cache.TryRemove(key, out _);
                    value = default;
                    return false;
                }

                if (cacheItem.Value is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var expirationTime = DateTime.UtcNow.Add(expiration ?? _defaultExpiration);
            var cacheItem = new CacheItem
            {
                Value = value,
                ExpirationTime = expirationTime
            };

            _cache[key] = cacheItem;
        }

        private class CacheItem
        {
            public object Value { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
}