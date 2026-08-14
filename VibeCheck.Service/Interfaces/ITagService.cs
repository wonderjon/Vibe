using VibeCheck.Service.Dtos.Tags;

namespace VibeCheck.Service.Interfaces;

public interface ITagService
{
    Task<IReadOnlyList<VibeTagDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
