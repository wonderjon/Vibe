using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Service.Common;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.Users;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;
using VibeCheck.Service.Validators;

namespace VibeCheck.Service.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;

    public UserService(IUnitOfWork uow, IValidator<UpdateProfileRequest> updateProfileValidator)
    {
        _uow = uow;
        _updateProfileValidator = updateProfileValidator;
    }

    public async Task<UserProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), userId);

        var counts = await GetCountsAsync(userId, cancellationToken);
        return user.ToProfileDto(counts.Followers, counts.Following, counts.VibeChecks);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _updateProfileValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), userId);

        user.DisplayName = request.DisplayName.Trim();
        user.Bio = request.Bio?.Trim();
        user.AvatarUrl = request.AvatarUrl;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        var counts = await GetCountsAsync(userId, cancellationToken);
        return user.ToProfileDto(counts.Followers, counts.Following, counts.VibeChecks);
    }

    public async Task<PublicUserDto> GetPublicProfileAsync(Guid targetUserId, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), targetUserId);

        var counts = await GetCountsAsync(targetUserId, cancellationToken);
        var isFollowed = currentUserId is not null && await _uow.Follows.AnyAsync(
            f => f.FollowerId == currentUserId && f.FollowingId == targetUserId, cancellationToken);

        return user.ToPublicDto(counts.Followers, counts.Following, counts.VibeChecks, isFollowed);
    }

    public async Task<PagedResult<VibeCheckDto>> GetUserVibeChecksAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _uow.VibeCheckEntries.Query().WithDetails()
            .Where(e => e.UserId == targetUserId)
            .OrderByDescending(e => e.CreatedAt);

        return await PageAsync(query, page, pageSize, e => e.ToDto(currentUserId), cancellationToken);
    }

    public async Task<PagedResult<PublicUserDto>> SearchUsersAsync(string query, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalized = query.Trim().ToLowerInvariant();

        var usersQuery = _uow.Users.Query()
            .Where(u => u.UserName.ToLower().Contains(normalized) || u.DisplayName.ToLower().Contains(normalized))
            .OrderBy(u => u.UserName);

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = new List<PublicUserDto>(users.Count);
        foreach (var user in users)
        {
            var counts = await GetCountsAsync(user.Id, cancellationToken);
            var isFollowed = currentUserId is not null && await _uow.Follows.AnyAsync(
                f => f.FollowerId == currentUserId && f.FollowingId == user.Id, cancellationToken);
            dtos.Add(user.ToPublicDto(counts.Followers, counts.Following, counts.VibeChecks, isFollowed));
        }

        return PagedResult<PublicUserDto>.Create(dtos, page, pageSize, totalCount);
    }

    public async Task FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
            throw new BadRequestException("You cannot follow yourself.");

        if (!await _uow.Users.AnyAsync(u => u.Id == targetUserId, cancellationToken))
            throw new NotFoundException(nameof(AppUser), targetUserId);

        var alreadyFollowing = await _uow.Follows.AnyAsync(
            f => f.FollowerId == currentUserId && f.FollowingId == targetUserId, cancellationToken);
        if (alreadyFollowing)
            return;

        await _uow.Follows.AddAsync(new Follow { FollowerId = currentUserId, FollowingId = targetUserId }, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var follow = await _uow.Follows.FirstOrDefaultAsync(
            f => f.FollowerId == currentUserId && f.FollowingId == targetUserId, cancellationToken);
        if (follow is null)
            return;

        _uow.Follows.Remove(follow);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<PublicUserDto>> GetFollowersAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _uow.Follows.Query()
            .Where(f => f.FollowingId == targetUserId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Follower);

        return await PageUsersAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<PublicUserDto>> GetFollowingAsync(Guid targetUserId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _uow.Follows.Query()
            .Where(f => f.FollowerId == targetUserId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.FollowingUser);

        return await PageUsersAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    private async Task<PagedResult<PublicUserDto>> PageUsersAsync(IQueryable<AppUser> query, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = new List<PublicUserDto>(users.Count);
        foreach (var user in users)
        {
            var counts = await GetCountsAsync(user.Id, cancellationToken);
            var isFollowed = currentUserId is not null && await _uow.Follows.AnyAsync(
                f => f.FollowerId == currentUserId && f.FollowingId == user.Id, cancellationToken);
            dtos.Add(user.ToPublicDto(counts.Followers, counts.Following, counts.VibeChecks, isFollowed));
        }

        return PagedResult<PublicUserDto>.Create(dtos, page, pageSize, totalCount);
    }

    private static async Task<PagedResult<TDto>> PageAsync<TEntity, TDto>(
        IOrderedQueryable<TEntity> query, int page, int pageSize, Func<TEntity, TDto> map, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResult<TDto>.Create(items.Select(map).ToList(), page, pageSize, totalCount);
    }

    private async Task<(int Followers, int Following, int VibeChecks)> GetCountsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var followers = await _uow.Follows.Query().CountAsync(f => f.FollowingId == userId, cancellationToken);
        var following = await _uow.Follows.Query().CountAsync(f => f.FollowerId == userId, cancellationToken);
        var vibeChecks = await _uow.VibeCheckEntries.Query().CountAsync(v => v.UserId == userId, cancellationToken);
        return (followers, following, vibeChecks);
    }
}
