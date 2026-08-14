using VibeCheck.Service.Dtos.Admin;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;

namespace VibeCheck.Service.Interfaces;

public interface IAdminService
{
    /// <summary>SuperAdmin only — enforced by the controller's [Authorize(Roles = "SuperAdmin")].</summary>
    Task<AdminDto> CreateAdminAsync(CreateAdminRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminDto>> GetAdminsAsync(CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task AssignVenueAsync(Guid adminId, Guid venueId, CancellationToken cancellationToken = default);

    Task UnassignVenueAsync(Guid adminId, Guid venueId, CancellationToken cancellationToken = default);

    // The methods below are reachable by Admin or SuperAdmin ([Authorize(Roles = "Admin,SuperAdmin")]),
    // but an Admin can only act on venues assigned to them — that check happens inside the service,
    // since [Authorize] alone can't express "only venues this specific admin manages".
    Task<PagedResult<VibeCheckDto>> GetVenueVisitorsAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task RemoveVisitorVibeCheckAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, Guid entryId, CancellationToken cancellationToken = default);

    Task<BannedUserDto> BanUserAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, BanUserRequest request, CancellationToken cancellationToken = default);

    Task UnbanUserAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BannedUserDto>> GetBannedUsersAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, CancellationToken cancellationToken = default);
}
