using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessingApi.Services
{
    public enum FilterType
    {
        Grayscale,
        Sepia,
        Invert
    }

    public interface IImageProcessingService
    {
        Task<(byte[] processedImage, string contentType)> ProcessImageAsync(IFormFile imageFile, FilterType filter);
    }

    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ICacheService _cacheService;

        public ImageProcessingService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<(byte[] processedImage, string contentType)> ProcessImageAsync(IFormFile imageFile, FilterType filter)
        {
            // Generate a cache key based on the image content and filter
            var cacheKey = await GenerateCacheKeyAsync(imageFile, filter);

            // Check if the processed image is in the cache
            if (_cacheService.TryGetValue<CachedImage>(cacheKey, out var cachedImage))
            {
                return (cachedImage.ImageData, cachedImage.ContentType);
            }

            // Process the image
            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);
            stream.Position = 0;

            using var image = Image.Load(stream);
            
            // Apply the requested filter
            switch (filter)
            {
                case FilterType.Grayscale:
                    image.Mutate(x => x.Grayscale());
                    break;
                case FilterType.Sepia:
                    image.Mutate(x => x.Sepia());
                    break;
                case FilterType.Invert:
                    image.Mutate(x => x.Invert());
                    break;
            }

            // Save the processed image to a memory stream
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat);
            var processedImageData = outputStream.ToArray();

            // Cache the processed image
            _cacheService.Set(cacheKey, new CachedImage
            {
                ImageData = processedImageData,
                ContentType = imageFile.ContentType
            }, TimeSpan.FromHours(24));

            return (processedImageData, imageFile.ContentType);
        }

        private async Task<string> GenerateCacheKeyAsync(IFormFile imageFile, FilterType filter)
        {
            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);
            stream.Position = 0;

            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hash = BitConverter.ToString(hashBytes).Replace("-", "");

            return $"{hash}_{filter}";
        }

        private class CachedImage
        {
            public byte[] ImageData { get; set; }
            public string ContentType { get; set; }
        }
    }
}