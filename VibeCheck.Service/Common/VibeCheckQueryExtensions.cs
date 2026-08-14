using Microsoft.EntityFrameworkCore;
using VibeCheck.Domain.Entities;

namespace VibeCheck.Service.Common;

/// <summary>Shared eager-loading shape for VibeCheckEntry, reused by every service that returns VibeCheckDto.</summary>
internal static class VibeCheckQueryExtensions
{
    public static IQueryable<VibeCheckEntry> WithDetails(this IQueryable<VibeCheckEntry> query)
        => query
            .Include(e => e.Venue)
            .Include(e => e.User)
            .Include(e => e.Photos)
            .Include(e => e.EntryTags).ThenInclude(t => t.VibeTag)
            .Include(e => e.Reactions)
            .Include(e => e.Comments);
}
