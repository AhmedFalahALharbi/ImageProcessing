using System;
using System.Collections.Generic;

namespace ImageProcessingApi.Services
{
    public interface IApiKeyService
    {
        string GenerateApiKey();
        bool IsValidApiKey(string apiKey);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly HashSet<string> _validApiKeys = new();

        public string GenerateApiKey()
        {
            string apiKey = Guid.NewGuid().ToString("N");
            _validApiKeys.Add(apiKey);
            return apiKey;
        }

        public bool IsValidApiKey(string apiKey)
        {
            return _validApiKeys.Contains(apiKey);
        }
    }
}