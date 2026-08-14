using VibeCheck.Domain.Enums;

namespace VibeCheck.Service.Dtos.Venues;

public record VenueDto(
    Guid Id,
    string Name,
    VenueCategory Category,
    string? Description,
    string Address,
    string City,
    double Latitude,
    double Longitude,
    string? CoverImageUrl,
    double AverageVibeScore,
    int TotalCheckIns,
    Guid CreatedByUserId,
    string CreatedByUserName,
    DateTime CreatedAt,
    bool IsSavedByCurrentUser,
    double? DistanceKm);

public record CreateVenueRequest(
    string Name,
    VenueCategory Category,
    string? Description,
    string Address,
    string City,
    double Latitude,
    double Longitude,
    string? CoverImageUrl);

public record UpdateVenueRequest(
    string Name,
    VenueCategory Category,
    string? Description,
    string Address,
    string City,
    string? CoverImageUrl);

public record VenueSearchQuery(
    string? Search,
    VenueCategory? Category,
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    int Page = 1,
    int PageSize = 20);
