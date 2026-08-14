using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Dtos.Venues;

namespace VibeCheck.Service.Interfaces;

public interface IVenueService
{
    Task<VenueDto> CreateAsync(Guid currentUserId, CreateVenueRequest request, CancellationToken cancellationToken = default);

    Task<VenueDto> GetByIdAsync(Guid venueId, Guid? currentUserId, CancellationToken cancellationToken = default);

    Task<VenueDto> UpdateAsync(Guid currentUserId, bool callerIsSuperAdmin, Guid venueId, UpdateVenueRequest request, CancellationToken cancellationToken = default);

    /// <summary>SuperAdmin only — enforced by the controller's [Authorize(Roles = "SuperAdmin")].</summary>
    Task DeleteAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<PagedResult<VenueDto>> SearchAsync(VenueSearchQuery query, Guid? currentUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VenueDto>> GetTrendingAsync(Guid? currentUserId, int take, CancellationToken cancellationToken = default);

    Task<PagedResult<VibeCheckDto>> GetVenueVibeChecksAsync(Guid venueId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task SaveAsync(Guid currentUserId, Guid venueId, CancellationToken cancellationToken = default);

    Task UnsaveAsync(Guid currentUserId, Guid venueId, CancellationToken cancellationToken = default);
}
