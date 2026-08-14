using Microsoft.EntityFrameworkCore;
using VibeCheck.Domain.Entities;

namespace VibeCheck.DataAcces.Persistence;

/// <summary>Seeds the fixed lookup data (vibe tags) the frontend's tag-picker UI needs. Idempotent.</summary>
public static class DbSeeder
{
    private static readonly string[] DefaultTags =
    [
        "Live Music", "DJ Set", "Long Line", "Cheap Drinks", "Happy Hour",
        "Great Views", "Outdoor Seating", "Dance Floor", "Chill Vibes", "Date Night",
        "Sports on TV", "Late Night", "Family Friendly", "Karaoke", "Rooftop"
    ];

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.VibeTags.AnyAsync(cancellationToken))
            return;

        context.VibeTags.AddRange(DefaultTags.Select(name => new VibeTag { Name = name }));
        await context.SaveChangesAsync(cancellationToken);
    }
}
