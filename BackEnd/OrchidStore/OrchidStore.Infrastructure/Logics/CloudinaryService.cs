using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchidStore.Application.Logics;

namespace OrchidStore.Infrastructure.Logics;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    /// <summary>
    /// Upload images to Cloudinary
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File not valid");
        }

        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream)
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        // Check for error
        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        // Return the URL
        return uploadResult.SecureUrl?.ToString() ?? throw new Exception("Upload failed");
    }
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}