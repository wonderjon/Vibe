using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Service.Common;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Dtos.Venues;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;
using VibeCheck.Service.Validators;

namespace VibeCheck.Service.Implementations;

public class VenueService : IVenueService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateVenueRequest> _createValidator;
    private readonly IValidator<UpdateVenueRequest> _updateValidator;

    public VenueService(IUnitOfWork uow, IValidator<CreateVenueRequest> createValidator, IValidator<UpdateVenueRequest> updateValidator)
    {
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<VenueDto> CreateAsync(Guid currentUserId, CreateVenueRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var creator = await _uow.Users.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), currentUserId);

        var venue = new Venue
        {
            Name = request.Name.Trim(),
            Category = request.Category,
            Description = request.Description?.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CoverImageUrl = request.CoverImageUrl,
            CreatedByUserId = currentUserId,
            CreatedByUser = creator
        };

        await _uow.Venues.AddAsync(venue, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return venue.ToDto();
    }

    public async Task<VenueDto> GetByIdAsync(Guid venueId, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        var venue = await _uow.Venues.Query().Include(v => v.CreatedByUser)
            .FirstOrDefaultAsync(v => v.Id == venueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), venueId);

        var isSaved = currentUserId is not null && await _uow.SavedVenues.AnyAsync(
            s => s.UserId == currentUserId && s.VenueId == venueId, cancellationToken);

        return venue.ToDto(isSaved);
    }

    public async Task<VenueDto> UpdateAsync(Guid currentUserId, bool callerIsSuperAdmin, Guid venueId, UpdateVenueRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var venue = await _uow.Venues.Query(tracked: true).Include(v => v.CreatedByUser)
            .FirstOrDefaultAsync(v => v.Id == venueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), venueId);

        if (venue.CreatedByUserId != currentUserId && !callerIsSuperAdmin)
        {
            var isAssignedAdmin = await _uow.VenueAdminAssignments.AnyAsync(
                a => a.AdminUserId == currentUserId && a.VenueId == venueId, cancellationToken);
            if (!isAssignedAdmin)
                throw new ForbiddenException("Only the venue's creator, an assigned admin, or a SuperAdmin can update it.");
        }

        venue.Name = request.Name.Trim();
        venue.Category = request.Category;
        venue.Description = request.Description?.Trim();
        venue.Address = request.Address.Trim();
        venue.City = request.City.Trim();
        venue.CoverImageUrl = request.CoverImageUrl;

        _uow.Venues.Update(venue);
        await _uow.SaveChangesAsync(cancellationToken);

        return venue.ToDto();
    }

    public async Task DeleteAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var venue = await _uow.Venues.Query(tracked: true).FirstOrDefaultAsync(v => v.Id == venueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), venueId);

        _uow.Venues.Remove(venue);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<VenueDto>> SearchAsync(VenueSearchQuery query, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var venuesQuery = _uow.Venues.Query().Include(v => v.CreatedByUser).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            venuesQuery = venuesQuery.Where(v => v.Name.ToLower().Contains(term) || v.City.ToLower().Contains(term));
        }

        if (query.Category is not null)
            venuesQuery = venuesQuery.Where(v => v.Category == query.Category);

        var savedIds = await GetSavedVenueIdsAsync(currentUserId, cancellationToken);

        // Nearby search: fetch the filtered candidate set into memory to apply Haversine
        // distance (no PostGIS in this MVP), then sort/paginate there.
        if (query.Latitude is { } lat && query.Longitude is { } lon)
        {
            var candidates = await venuesQuery.ToListAsync(cancellationToken);
            var withDistance = candidates
                .Select(v => (Venue: v, Distance: GeoUtils.DistanceKm(lat, lon, v.Latitude, v.Longitude)))
                .Where(x => query.RadiusKm is not { } radius || x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .ToList();

            var totalCount = withDistance.Count;
            var page_ = withDistance.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => x.Venue.ToDto(savedIds.Contains(x.Venue.Id), Math.Round(x.Distance, 2)))
                .ToList();

            return PagedResult<VenueDto>.Create(page_, page, pageSize, totalCount);
        }

        venuesQuery = venuesQuery.OrderByDescending(v => v.CreatedAt);
        var total = await venuesQuery.CountAsync(cancellationToken);
        var items = await venuesQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<VenueDto>.Create(
            items.Select(v => v.ToDto(savedIds.Contains(v.Id))).ToList(), page, pageSize, total);
    }

    public async Task<IReadOnlyList<VenueDto>> GetTrendingAsync(Guid? currentUserId, int take, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddHours(-24);

        var trendingIds = await _uow.VibeCheckEntries.Query()
            .Where(e => e.CreatedAt >= since)
            .GroupBy(e => e.VenueId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (trendingIds.Count < take)
        {
            var backfillIds = await _uow.Venues.Query()
                .Where(v => !trendingIds.Contains(v.Id))
                .OrderByDescending(v => v.TotalCheckIns)
                .Select(v => v.Id)
                .Take(take - trendingIds.Count)
                .ToListAsync(cancellationToken);
            trendingIds.AddRange(backfillIds);
        }

        var venues = await _uow.Venues.Query().Include(v => v.CreatedByUser)
            .Where(v => trendingIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        var savedIds = await GetSavedVenueIdsAsync(currentUserId, cancellationToken);
        var byId = venues.ToDictionary(v => v.Id);

        return trendingIds
            .Where(byId.ContainsKey)
            .Select(id => byId[id].ToDto(savedIds.Contains(id)))
            .ToList();
    }

    public async Task<PagedResult<VibeCheckDto>> GetVenueVibeChecksAsync(Guid venueId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!await _uow.Venues.AnyAsync(v => v.Id == venueId, cancellationToken))
            throw new NotFoundException(nameof(Venue), venueId);

        var query = _uow.VibeCheckEntries.Query().WithDetails()
            .Where(e => e.VenueId == venueId)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<VibeCheckDto>.Create(
            items.Select(e => e.ToDto(currentUserId)).ToList(), page, pageSize, totalCount);
    }

    public async Task SaveAsync(Guid currentUserId, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (!await _uow.Venues.AnyAsync(v => v.Id == venueId, cancellationToken))
            throw new NotFoundException(nameof(Venue), venueId);

        var alreadySaved = await _uow.SavedVenues.AnyAsync(
            s => s.UserId == currentUserId && s.VenueId == venueId, cancellationToken);
        if (alreadySaved)
            return;

        await _uow.SavedVenues.AddAsync(new SavedVenue { UserId = currentUserId, VenueId = venueId }, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task UnsaveAsync(Guid currentUserId, Guid venueId, CancellationToken cancellationToken = default)
    {
        var saved = await _uow.SavedVenues.FirstOrDefaultAsync(
            s => s.UserId == currentUserId && s.VenueId == venueId, cancellationToken);
        if (saved is null)
            return;

        _uow.SavedVenues.Remove(saved);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    private async Task<HashSet<Guid>> GetSavedVenueIdsAsync(Guid? currentUserId, CancellationToken cancellationToken)
    {
        if (currentUserId is null)
            return [];

        var ids = await _uow.SavedVenues.Query()
            .Where(s => s.UserId == currentUserId)
            .Select(s => s.VenueId)
            .ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }
}
