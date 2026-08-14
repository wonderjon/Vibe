using Microsoft.Extensions.Options;
using VibeCheck.Service.Exceptions;

namespace VibeCheck.Service.Storage;

/// <summary>
/// MVP implementation: saves to local disk under wwwroot/uploads, served as static files.
/// Swap for an S3/Cloudinary-backed implementation later without touching any controller.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (content.Length > _options.MaxFileSizeBytes)
            throw new BadRequestException($"File exceeds the {_options.MaxFileSizeBytes / (1024 * 1024)}MB limit.");

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            throw new BadRequestException($"File type '{extension}' is not allowed.");

        Directory.CreateDirectory(_options.RootPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_options.RootPath, fileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{fileName}";
    }
}
