# Rate-Limited Image Processing API

A RESTful API built with .NET Core that allows users to upload images and apply simple filters (grayscale, sepia, invert). The API includes rate limiting to prevent abuse, caching to improve performance, and API key authentication for basic security.

## Features

- **Image Processing**: Upload and apply filters to images
- **API Key Authentication**: Generate and validate API keys for access control
- **Rate Limiting**: Restrict the number of requests per API key within a time window
- **Response Caching**: Cache processed images to improve performance
- **Swagger Documentation**: Interactive API documentation and testing interface

## Technologies Used

- .NET
- SixLabors.ImageSharp for image processing
- Swashbuckle for Swagger/OpenAPI documentation

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or newer
- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/) (optional)

## Setup and Installation

1. **Clone the repository**
   ```bash
   git clone 
   cd image-processing-api
   ```

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the Swagger UI**
   
   Open your browser and navigate to:
   ```
   https://localhost:7xxx/
   ```
   (The exact port may vary based on your configuration)

## API Endpoints

### Generate an API Key

```
POST /api/apikeys/generate
```

**Response**:
```json
{
  "apiKey": "generated-api-key-guid"
}
```

### Process an Image

```
POST /api/images/process?filter=Grayscale
```

**Headers**:
- `X-API-Key`: Your API key

**Query Parameters**:
- `filter`: The filter to apply (Grayscale, Sepia, Invert)

**Body**:
- `imageFile`: The image file to process (multipart/form-data)

**Response**:
- The processed image file

## Configuration

Configuration options are available in `appsettings.json`:

```json
{
  "RateLimitConfig": {
    "RequestLimit": 100,
    "TimeWindowInMinutes": 60
  }
}
```

- `RequestLimit`: Maximum number of requests allowed per API key
- `TimeWindowInMinutes`: Time window for rate limiting in minutes

## Usage Examples

### Using cURL

1. Generate an API key:
   ```bash
   curl -X POST https://localhost:7xxx/api/apikeys/generate
   ```

2. Process an image:
   ```bash
   curl -X POST https://localhost:7xxx/api/images/process?filter=Grayscale \
     -H "X-API-Key: your-api-key" \
     -F "imageFile=@/path/to/your/image.jpg"
   ```

### Using Swagger UI

1. Navigate to the Swagger UI at `https://localhost:7xxx/`
2. Generate an API key using the `/api/apikeys/generate` endpoint
3. Click "Authorize" at the top of the page and enter your API key
4. Use the `/api/images/process` endpoint to upload and process an image

## Project Structure

```
ImageProcessingApi/
├── Controllers/
│   ├── ApiKeyController.cs      # API key generation endpoint
│   └── ImageController.cs       # Image processing endpoint
├── Middleware/
│   ├── ApiKeyAuthenticationMiddleware.cs  # API key validation
│   └── RateLimitingMiddleware.cs          # Request rate limiting
├── Services/
│   ├── ApiKeyService.cs         # API key management
│   ├── CacheService.cs          # Image caching
│   └── ImageProcessingService.cs # Image processing logic
├── Models/
│   └── RateLimitConfig.cs       # Rate limiting configuration
├── Program.cs                   # Application entry point
└── appsettings.json             # Configuration settings
```

## Testing

### Rate Limiting Testing

Make multiple requests in quick succession to test the rate limiting functionality. After exceeding the configured limit (default: 100 requests per hour), you should receive a 429 Too Many Requests response.

### Caching Testing

Upload the same image with the same filter multiple times. The second and subsequent requests should be faster as the processed image is retrieved from the cache.

## Troubleshooting

### HTTPS Certificate Issues

If you encounter HTTPS certificate errors, run:
```bash
dotnet dev-certs https --trust
```

### Image Format Issues

Make sure the uploaded image has a valid format (JPEG, PNG, GIF, etc.) and the content type is correctly set.

### Rate Limiting Issues

If you're getting rate limit errors during testing, you can modify the `RateLimitConfig` in `appsettings.json` to increase the limits.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
