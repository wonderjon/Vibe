using VibeCheck.Domain.Entities;
using VibeCheck.Service.Dtos.Venues;

namespace VibeCheck.Service.Mapping;

public static class VenueMappingExtensions
{
    public static VenueDto ToDto(this Venue venue, bool isSavedByCurrentUser = false, double? distanceKm = null)
        => new(venue.Id, venue.Name, venue.Category, venue.Description, venue.Address, venue.City,
            venue.Latitude, venue.Longitude, venue.CoverImageUrl, venue.AverageVibeScore, venue.TotalCheckIns,
            venue.CreatedByUserId, venue.CreatedByUser?.UserName ?? string.Empty, venue.CreatedAt,
            isSavedByCurrentUser, distanceKm);
}
