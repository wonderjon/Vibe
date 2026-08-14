using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Domain.Enums;
using VibeCheck.Service.Dtos.Auth;
using VibeCheck.Service.Exceptions;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Mapping;
using VibeCheck.Service.Security;
using VibeCheck.Service.Validators;

namespace VibeCheck.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher<AppUser> passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUserName = request.UserName.Trim();

        if (await _uow.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        if (await _uow.Users.AnyAsync(u => u.UserName == normalizedUserName, cancellationToken))
            throw new ConflictException("This username is already taken.");

        // Role is never taken from the client — self-registration always creates a Customer.
        // Admin accounts are created only by a SuperAdmin via IAdminService; SuperAdmin is seeded once at startup.
        var user = new AppUser
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Customer
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid email or password.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existing = await _uow.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        if (existing is null || !existing.IsActive)
            throw new UnauthorizedException("Refresh token is invalid or expired.");

        var user = await _uow.Users.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token is invalid or expired.");

        var newRefreshValue = _jwtTokenService.GenerateRefreshTokenValue();
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByToken = newRefreshValue;
        _uow.RefreshTokens.Update(existing);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        await _uow.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var counts = await GetUserCountsAsync(user.Id, cancellationToken);

        return new AuthResponse(accessToken, expiresAt, newRefreshValue,
            user.ToProfileDto(counts.Followers, counts.Following, counts.VibeChecks));
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existing = await _uow.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        if (existing is null || existing.RevokedAt is not null)
            return;

        existing.RevokedAt = DateTime.UtcNow;
        _uow.RefreshTokens.Update(existing);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _forgotPasswordValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            ?? throw new NotFoundException("No account found with this email.");

        // MVP: return the token directly instead of emailing it. Swap for a real email
        // provider before going to production, and stop returning it in the response.
        return _jwtTokenService.GeneratePasswordResetToken(user.Id);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _resetPasswordValidator.ValidateAndThrowAppAsync(request, cancellationToken);

        var userId = _jwtTokenService.ValidatePasswordResetToken(request.ResetToken)
            ?? throw new BadRequestException("Reset token is invalid or expired.");

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new BadRequestException("Reset token is invalid or expired.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        _uow.Users.Update(user);

        // Force re-login everywhere for safety.
        var activeTokens = await _uow.RefreshTokens.Query(tracked: true)
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(AppUser user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshValue = _jwtTokenService.GenerateRefreshTokenValue();

        await _uow.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshValue,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var counts = await GetUserCountsAsync(user.Id, cancellationToken);
        return new AuthResponse(accessToken, expiresAt, refreshValue,
            user.ToProfileDto(counts.Followers, counts.Following, counts.VibeChecks));
    }

    private async Task<(int Followers, int Following, int VibeChecks)> GetUserCountsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var followers = await _uow.Follows.Query().CountAsync(f => f.FollowingId == userId, cancellationToken);
        var following = await _uow.Follows.Query().CountAsync(f => f.FollowerId == userId, cancellationToken);
        var vibeChecks = await _uow.VibeCheckEntries.Query().CountAsync(v => v.UserId == userId, cancellationToken);
        return (followers, following, vibeChecks);
    }
}
