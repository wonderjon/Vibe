using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Domain.Enums;
using VibeCheck.Service.Common;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;
using VibeCheck.Service.Validators;

namespace VibeCheck.Service.Implementations;

public class VibeCheckService : IVibeCheckService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateVibeCheckRequest> _createValidator;
    private readonly IValidator<CreateCommentRequest> _commentValidator;

    public VibeCheckService(IUnitOfWork uow, IValidator<CreateVibeCheckRequest> createValidator, IValidator<CreateCommentRequest> commentValidator)
    {
        _uow = uow;
        _createValidator = createValidator;
        _commentValidator = commentValidator;
    }

    public async Task<VibeCheckDto> CreateAsync(Guid currentUserId, CreateVibeCheckRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var venue = await _uow.Venues.Query(tracked: true).FirstOrDefaultAsync(v => v.Id == request.VenueId, cancellationToken)
            ?? throw new NotFoundException(nameof(Venue), request.VenueId);

        if (await _uow.VenueBans.AnyAsync(b => b.VenueId == request.VenueId && b.UserId == currentUserId, cancellationToken))
            throw new ForbiddenException("You are banned from posting vibe checks at this venue.");

        var entry = new VibeCheckEntry
        {
            VenueId = venue.Id,
            UserId = currentUserId,
            VibeScore = request.VibeScore,
            CrowdLevel = request.CrowdLevel,
            Comment = request.Comment?.Trim(),
            ExpiresAt = DateTime.UtcNow.AddHours(4)
        };

        if (request.PhotoUrls is { Count: > 0 })
        {
            foreach (var url in request.PhotoUrls)
                entry.Photos.Add(new VibeCheckPhoto { Url = url, VibeCheckEntry = entry });
        }

        if (request.TagIds is { Count: > 0 })
        {
            var validTagIds = await _uow.VibeTags.Query()
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            foreach (var tagId in validTagIds)
                entry.EntryTags.Add(new VibeCheckEntryTag { VibeCheckEntryId = entry.Id, VibeTagId = tagId, VibeCheckEntry = entry });
        }

        await _uow.VibeCheckEntries.AddAsync(entry, cancellationToken);

        // Recompute the venue's "live" average from currently-active entries plus this new one,
        // and bump the lifetime activity counter — all in the same SaveChanges as the insert.
        var activeScores = await _uow.VibeCheckEntries.Query()
            .Where(e => e.VenueId == venue.Id && e.ExpiresAt > DateTime.UtcNow)
            .Select(e => e.VibeScore)
            .ToListAsync(cancellationToken);
        activeScores.Add(request.VibeScore);

        venue.AverageVibeScore = Math.Round(activeScores.Average(), 2);
        venue.TotalCheckIns += 1;
        _uow.Venues.Update(venue);

        await _uow.SaveChangesAsync(cancellationToken);

        var saved = await _uow.VibeCheckEntries.Query().WithDetails().FirstAsync(e => e.Id == entry.Id, cancellationToken);
        return saved.ToDto(currentUserId);
    }

    public async Task<VibeCheckDto> GetByIdAsync(Guid id, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        var entry = await _uow.VibeCheckEntries.Query().WithDetails()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(VibeCheckEntry), id);

        return entry.ToDto(currentUserId);
    }

    public async Task DeleteAsync(Guid currentUserId, Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _uow.VibeCheckEntries.Query(tracked: true)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(VibeCheckEntry), id);

        if (entry.UserId != currentUserId)
            throw new ForbiddenException("You can only delete your own vibe checks.");

        var venue = await _uow.Venues.Query(tracked: true).FirstAsync(v => v.Id == entry.VenueId, cancellationToken);

        _uow.VibeCheckEntries.Remove(entry);
        await _uow.SaveChangesAsync(cancellationToken);

        await VenueAggregateRecomputer.RecomputeAfterRemovalAsync(_uow, venue, cancellationToken);
    }

    public async Task<PagedResult<VibeCheckDto>> GetFollowingFeedAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var followingIds = _uow.Follows.Query()
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId);

        var query = _uow.VibeCheckEntries.Query().WithDetails()
            .Where(e => followingIds.Contains(e.UserId))
            .OrderByDescending(e => e.CreatedAt);

        return await PageAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<VibeCheckDto>> GetGlobalFeedAsync(Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _uow.VibeCheckEntries.Query().WithDetails()
            .OrderByDescending(e => e.CreatedAt);

        return await PageAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    public async Task ReactAsync(Guid currentUserId, Guid entryId, ReactionType type, CancellationToken cancellationToken = default)
    {
        if (!await _uow.VibeCheckEntries.AnyAsync(e => e.Id == entryId, cancellationToken))
            throw new NotFoundException(nameof(VibeCheckEntry), entryId);

        var existing = await _uow.VibeCheckReactions.FirstOrDefaultAsync(
            r => r.VibeCheckEntryId == entryId && r.UserId == currentUserId, cancellationToken);

        if (existing is null)
        {
            await _uow.VibeCheckReactions.AddAsync(
                new VibeCheckReaction { VibeCheckEntryId = entryId, UserId = currentUserId, Type = type }, cancellationToken);
        }
        else if (existing.Type == type)
        {
            _uow.VibeCheckReactions.Remove(existing); // reacting the same way again toggles it off
        }
        else
        {
            existing.Type = type;
            _uow.VibeCheckReactions.Update(existing);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<VibeCheckCommentDto> AddCommentAsync(Guid currentUserId, Guid entryId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        await _commentValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        if (!await _uow.VibeCheckEntries.AnyAsync(e => e.Id == entryId, cancellationToken))
            throw new NotFoundException(nameof(VibeCheckEntry), entryId);

        var user = await _uow.Users.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(AppUser), currentUserId);

        var comment = new VibeCheckComment
        {
            VibeCheckEntryId = entryId,
            UserId = currentUserId,
            Text = request.Text.Trim(),
            User = user
        };
        await _uow.VibeCheckComments.AddAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return comment.ToDto();
    }

    public async Task<PagedResult<VibeCheckCommentDto>> GetCommentsAsync(Guid entryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _uow.VibeCheckComments.Query()
            .Include(c => c.User)
            .Where(c => c.VibeCheckEntryId == entryId)
            .OrderBy(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<VibeCheckCommentDto>.Create(items.Select(c => c.ToDto()).ToList(), page, pageSize, totalCount);
    }

    public async Task DeleteCommentAsync(Guid currentUserId, Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _uow.VibeCheckComments.Query(tracked: true)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken)
            ?? throw new NotFoundException(nameof(VibeCheckComment), commentId);

        if (comment.UserId != currentUserId)
            throw new ForbiddenException("You can only delete your own comments.");

        _uow.VibeCheckComments.Remove(comment);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    private static async Task<PagedResult<VibeCheckDto>> PageAsync(
        IOrderedQueryable<VibeCheckEntry> query, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResult<VibeCheckDto>.Create(items.Select(e => e.ToDto(currentUserId)).ToList(), page, pageSize, totalCount);
    }
}
