namespace VibeCheck.Service.Storage;

/// <summary>
/// Kept decoupled from IWebHostEnvironment so the Service layer stays framework-agnostic;
/// the API layer resolves the real paths at startup and configures this via DI.
/// </summary>
public class StorageOptions
{
    public string RootPath { get; set; } = string.Empty;

    public string PublicBaseUrl { get; set; } = "/uploads";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
}
