namespace VibeCheck.Service.Storage;

public interface IFileStorageService
{
    /// <summary>Saves a file and returns its public URL. Throws BadRequestException on invalid input.</summary>
    Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default);
}
