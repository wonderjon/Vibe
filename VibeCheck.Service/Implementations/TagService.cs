using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Service.Dtos.Tags;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;

namespace VibeCheck.Service.Implementations;

public class TagService : ITagService
{
    private readonly IUnitOfWork _uow;

    public TagService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<VibeTagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _uow.VibeTags.Query().OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return tags.Select(t => t.ToDto()).ToList();
    }
}
