namespace VibeCheck.Service.Dtos.Admin;

public record CreateAdminRequest(string Email, string Password, string DisplayName, IReadOnlyList<Guid> VenueIds);

public record VenueSummaryDto(Guid Id, string Name);

public record AdminDto(Guid Id, string Email, string DisplayName, DateTime CreatedAt, IReadOnlyList<VenueSummaryDto> AssignedVenues);

public record AssignVenueRequest(Guid VenueId);

public record BanUserRequest(Guid UserId, string? Reason);

public record BannedUserDto(Guid UserId, string UserName, string? Reason, DateTime BannedAt);
