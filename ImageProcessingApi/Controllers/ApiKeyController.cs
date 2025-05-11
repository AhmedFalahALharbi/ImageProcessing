using ImageProcessingApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessingApi.Controllers
{
    [ApiController]
    [Route("api/apikeys")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost("generate")]
        public IActionResult GenerateApiKey()
        {
            string apiKey = _apiKeyService.GenerateApiKey();
            return Ok(new { apiKey });
        }
    }
}