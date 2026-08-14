using System.Security.Claims;

namespace VibeCheckAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Use on [Authorize]-protected endpoints, where the id claim is guaranteed present.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Authenticated request is missing a valid user id claim.");
    }

    /// <summary>Use on endpoints that allow anonymous access but personalize the response when a valid token is present.</summary>
    public static Guid? GetUserIdOrNull(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    /// <summary>
    /// For endpoints reachable by both Admin and SuperAdmin where a SuperAdmin should bypass the
    /// per-venue assignment check — [Authorize(Roles=...)] alone can't express that distinction.
    /// </summary>
    public static bool IsSuperAdmin(this ClaimsPrincipal principal) => principal.IsInRole("SuperAdmin");
}
