using Microsoft.AspNetCore.Http;

namespace OrchidStore.Application.Logics;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
}