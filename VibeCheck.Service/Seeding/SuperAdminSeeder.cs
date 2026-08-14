using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Persistence;
using VibeCheck.Domain.Entities;
using VibeCheck.Domain.Enums;

namespace VibeCheck.Service.Seeding;

/// <summary>
/// Creates exactly one SuperAdmin account from configured credentials, if none exists yet.
/// There is no API endpoint that can create a SuperAdmin — this seeder is the only path,
/// which is what actually guarantees "only one, ever" rather than just convention.
/// </summary>
public static class SuperAdminSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher<AppUser> passwordHasher, string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (await context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin, cancellationToken))
            return;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // If something already occupies that email/username (e.g. a Customer registered first),
        // don't silently overwrite or duplicate it — surface it as a startup log instead of crashing.
        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail || u.UserName == "superadmin", cancellationToken))
            return;

        var superAdmin = new AppUser
        {
            UserName = "superadmin",
            Email = normalizedEmail,
            DisplayName = "Super Admin",
            Role = UserRole.SuperAdmin
        };
        superAdmin.PasswordHash = passwordHasher.HashPassword(superAdmin, password);

        context.Users.Add(superAdmin);
        await context.SaveChangesAsync(cancellationToken);
    }
}
