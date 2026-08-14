using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Domain.Enums;
using VibeCheck.Service.Common;
using VibeCheck.Service.Dtos.Admin;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;
using VibeCheck.Service.Validators;

namespace VibeCheck.Service.Implementations;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IValidator<CreateAdminRequest> _createAdminValidator;
    private readonly IValidator<BanUserRequest> _banUserValidator;

    public AdminService(
        IUnitOfWork uow,
        IPasswordHasher<AppUser> passwordHasher,
        IValidator<CreateAdminRequest> createAdminValidator,
        IValidator<BanUserRequest> banUserValidator)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _createAdminValidator = createAdminValidator;
        _banUserValidator = banUserValidator;
    }

    public async Task<AdminDto> CreateAdminAsync(CreateAdminRequest request, CancellationToken cancellationToken = default)
    {
        await _createAdminValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _uow.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var venues = await _uow.Venues.Query()
            .Where(v => request.VenueIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        if (venues.Count != request.VenueIds.Distinct().Count())
            throw new NotFoundException("One or more of the given venues does not exist.");

        var admin = new AppUser
        {
            UserName = await GenerateUniqueUserNameAsync(normalizedEmail, cancellationToken),
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Admin
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, request.Password);

        await _uow.Users.AddAsync(admin, cancellationToken);

        // Scalar FKs only — `venues` came from a no-tracking Query(), so attaching those
        // instances via the Venue navigation would make EF think they're new rows to insert
        // (duplicate PK on save). The FK value alone is enough for the assignment row.
        foreach (var venue in venues)
        {
            await _uow.VenueAdminAssignments.AddAsync(
                new VenueAdminAssignment { AdminUserId = admin.Id, VenueId = venue.Id },
                cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return new AdminDto(admin.Id, admin.Email, admin.DisplayName, admin.CreatedAt,
            venues.Select(v => new VenueSummaryDto(v.Id, v.Name)).ToList());
    }

    public async Task<IReadOnlyList<AdminDto>> GetAdminsAsync(CancellationToken cancellationToken = default)
    {
        var admins = await _uow.Users.Query()
            .Where(u => u.Role == UserRole.Admin)
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);

        var adminIds = admins.Select(a => a.Id).ToList();
        var assignments = await _uow.VenueAdminAssignments.Query()
            .Include(a => a.Venue)
            .Where(a => adminIds.Contains(a.AdminUserId))
            .ToListAsync(cancellationToken);

        return admins.Select(a => new AdminDto(
            a.Id, a.Email, a.DisplayName, a.CreatedAt,
            assignments.Where(x => x.AdminUserId == a.Id)
                .Select(x => new VenueSummaryDto(x.VenueId, x.Venue.Name))
                .ToList())
        ).ToList();
    }

    public async Task DeleteAdminAsync(Guid adminId, CancellationToken cancellationToken = default)
    {
        var admin = await _uow.Users.FirstOrDefaultAsync(u => u.Id == adminId && u.Role == UserRole.Admin, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), adminId);

        _uow.Users.Remove(admin);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignVenueAsync(Guid adminId, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (!await _uow.Users.AnyAsync(u => u.Id == adminId && u.Role == UserRole.Admin, cancellationToken))
            throw new NotFoundException(nameof(AppUser), adminId);

        if (!await _uow.Venues.AnyAsync(v => v.Id == venueId, cancellationToken))
            throw new NotFoundException(nameof(Venue), venueId);

        if (await _uow.VenueAdminAssignments.AnyAsync(a => a.AdminUserId == adminId && a.VenueId == venueId, cancellationToken))
            return;

        await _uow.VenueAdminAssignments.AddAsync(new VenueAdminAssignment { AdminUserId = adminId, VenueId = venueId }, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task UnassignVenueAsync(Guid adminId, Guid venueId, CancellationToken cancellationToken = default)
    {
        var assignment = await _uow.VenueAdminAssignments.FirstOrDefaultAsync(
            a => a.AdminUserId == adminId && a.VenueId == venueId, cancellationToken);
        if (assignment is null)
            return;

        _uow.VenueAdminAssignments.Remove(assignment);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<VibeCheckDto>> GetVenueVisitorsAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!await _uow.Venues.AnyAsync(v => v.Id == venueId, cancellationToken))
            throw new NotFoundException(nameof(Venue), venueId);

        await EnsureCanManageVenueAsync(callerId, callerIsSuperAdmin, venueId, cancellationToken);

        var query = _uow.VibeCheckEntries.Query().WithDetails()
            .Where(e => e.VenueId == venueId)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<VibeCheckDto>.Create(items.Select(e => e.ToDto(callerId)).ToList(), page, pageSize, totalCount);
    }

    public async Task RemoveVisitorVibeCheckAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, Guid entryId, CancellationToken cancellationToken = default)
    {
        await EnsureCanManageVenueAsync(callerId, callerIsSuperAdmin, venueId, cancellationToken);

        var entry = await _uow.VibeCheckEntries.Query(tracked: true)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.VenueId == venueId, cancellationToken)
            ?? throw new NotFoundException(nameof(VibeCheckEntry), entryId);

        var venue = await _uow.Venues.Query(tracked: true).FirstAsync(v => v.Id == venueId, cancellationToken);

        _uow.VibeCheckEntries.Remove(entry);
        await _uow.SaveChangesAsync(cancellationToken);

        await VenueAggregateRecomputer.RecomputeAfterRemovalAsync(_uow, venue, cancellationToken);
    }

    public async Task<BannedUserDto> BanUserAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, BanUserRequest request, CancellationToken cancellationToken = default)
    {
        await _banUserValidator.ValidateAndThrowAppAsync(request, cancellationToken);
        await EnsureCanManageVenueAsync(callerId, callerIsSuperAdmin, venueId, cancellationToken);

        if (!await _uow.Venues.AnyAsync(v => v.Id == venueId, cancellationToken))
            throw new NotFoundException(nameof(Venue), venueId);

        var targetUser = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), request.UserId);

        var existing = await _uow.VenueBans.FirstOrDefaultAsync(b => b.VenueId == venueId && b.UserId == request.UserId, cancellationToken);
        if (existing is not null)
            return new BannedUserDto(targetUser.Id, targetUser.UserName, existing.Reason, existing.CreatedAt);

        var ban = new VenueBan
        {
            VenueId = venueId,
            UserId = request.UserId,
            BannedByAdminUserId = callerId,
            Reason = request.Reason?.Trim()
        };
        await _uow.VenueBans.AddAsync(ban, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new BannedUserDto(targetUser.Id, targetUser.UserName, ban.Reason, ban.CreatedAt);
    }

    public async Task UnbanUserAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureCanManageVenueAsync(callerId, callerIsSuperAdmin, venueId, cancellationToken);

        var ban = await _uow.VenueBans.FirstOrDefaultAsync(b => b.VenueId == venueId && b.UserId == userId, cancellationToken);
        if (ban is null)
            return;

        _uow.VenueBans.Remove(ban);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BannedUserDto>> GetBannedUsersAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, CancellationToken cancellationToken = default)
    {
        await EnsureCanManageVenueAsync(callerId, callerIsSuperAdmin, venueId, cancellationToken);

        var bans = await _uow.VenueBans.Query()
            .Include(b => b.User)
            .Where(b => b.VenueId == venueId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return bans.Select(b => new BannedUserDto(b.UserId, b.User.UserName, b.Reason, b.CreatedAt)).ToList();
    }

    private async Task EnsureCanManageVenueAsync(Guid callerId, bool callerIsSuperAdmin, Guid venueId, CancellationToken cancellationToken)
    {
        if (callerIsSuperAdmin)
            return;

        var isAssigned = await _uow.VenueAdminAssignments.AnyAsync(
            a => a.AdminUserId == callerId && a.VenueId == venueId, cancellationToken);
        if (!isAssigned)
            throw new ForbiddenException("You are not an admin for this venue.");
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email, CancellationToken cancellationToken)
    {
        var basis = new string(email.Split('@')[0].Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(basis))
            basis = "admin";
        if (basis.Length > 28)
            basis = basis[..28];

        var candidate = basis;
        var suffix = 1;
        while (await _uow.Users.AnyAsync(u => u.UserName == candidate, cancellationToken))
        {
            candidate = $"{basis}{suffix}";
            suffix++;
        }

        return candidate;
    }
}
