using ImageProcessingApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ImageProcessingApi.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly IImageProcessingService _imageProcessingService;

        public ImageController(IImageProcessingService imageProcessingService)
        {
            _imageProcessingService = imageProcessingService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessImage(IFormFile imageFile, [FromQuery] string filter = "Grayscale")
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file was provided");
            }

            // Validate content type
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("The uploaded file is not an image");
            }

            // Parse filter type
            if (!Enum.TryParse<FilterType>(filter, true, out var filterType))
            {
                return BadRequest($"Invalid filter type. Available filters: {string.Join(", ", Enum.GetNames<FilterType>())}");
            }

            try
            {
                var (processedImage, contentType) = await _imageProcessingService.ProcessImageAsync(imageFile, filterType);
                return File(processedImage, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing image: {ex.Message}");
            }
        }
    }
}