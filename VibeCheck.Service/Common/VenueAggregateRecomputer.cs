using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;

namespace VibeCheck.Service.Common;

/// <summary>
/// Recomputes a venue's denormalized AverageVibeScore/TotalCheckIns after a vibe check entry is
/// removed — shared by VibeCheckService (owner deleting their own post) and AdminService
/// (admin/SuperAdmin removing someone else's post), so the aggregate logic exists exactly once.
/// </summary>
internal static class VenueAggregateRecomputer
{
    public static async Task RecomputeAfterRemovalAsync(IUnitOfWork uow, Venue venue, CancellationToken cancellationToken)
    {
        var remainingScores = await uow.VibeCheckEntries.Query()
            .Where(e => e.VenueId == venue.Id && e.ExpiresAt > DateTime.UtcNow)
            .Select(e => e.VibeScore)
            .ToListAsync(cancellationToken);

        venue.TotalCheckIns = Math.Max(0, venue.TotalCheckIns - 1);
        venue.AverageVibeScore = remainingScores.Count == 0 ? 0 : Math.Round(remainingScores.Average(), 2);
        uow.Venues.Update(venue);
        await uow.SaveChangesAsync(cancellationToken);
    }
}
