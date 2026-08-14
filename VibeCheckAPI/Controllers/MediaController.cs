using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Media;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Storage;

namespace VibeCheckAPI.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public MediaController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Uploads an image (avatar / venue cover / vibe check photo) and returns its public URL.
    /// Behind IFileStorageService so the MVP local-disk implementation can be swapped for S3/Cloudinary later.
    /// </summary>
    [Authorize]
    [HttpPost("upload")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file was provided.");

        await using var stream = file.OpenReadStream();
        var url = await _fileStorageService.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
        return Ok(new UploadResultDto(url));
    }
}
