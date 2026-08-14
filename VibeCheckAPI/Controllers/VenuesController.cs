using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Admin;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Dtos.Venues;
using VibeCheck.Service.Interfaces;
using VibeCheckAPI.Extensions;

namespace VibeCheckAPI.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;
    private readonly IAdminService _adminService;

    public VenuesController(IVenueService venueService, IAdminService adminService)
    {
        _venueService = venueService;
        _adminService = adminService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<VenueDto>> Create(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        var dto = await _venueService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VenueDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _venueService.GetByIdAsync(id, User.GetUserIdOrNull(), cancellationToken));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VenueDto>> Update(Guid id, UpdateVenueRequest request, CancellationToken cancellationToken)
        => Ok(await _venueService.UpdateAsync(User.GetUserId(), User.IsSuperAdmin(), id, request, cancellationToken));

    /// <summary>SuperAdmin only — full deletion, unlike the softer per-venue admin actions below.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _venueService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VenueDto>>> Search([FromQuery] VenueSearchQuery query, CancellationToken cancellationToken)
        => Ok(await _venueService.SearchAsync(query, User.GetUserIdOrNull(), cancellationToken));

    [HttpGet("trending")]
    public async Task<ActionResult<IReadOnlyList<VenueDto>>> Trending([FromQuery] int take = 10, CancellationToken cancellationToken = default)
        => Ok(await _venueService.GetTrendingAsync(User.GetUserIdOrNull(), take, cancellationToken));

    [HttpGet("{id:guid}/vibe-checks")]
    public async Task<ActionResult<PagedResult<VibeCheckDto>>> GetVibeChecks(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _venueService.GetVenueVibeChecksAsync(id, User.GetUserIdOrNull(), page, pageSize, cancellationToken));

    [Authorize]
    [HttpPost("{id:guid}/save")]
    public async Task<IActionResult> Save(Guid id, CancellationToken cancellationToken)
    {
        await _venueService.SaveAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/save")]
    public async Task<IActionResult> Unsave(Guid id, CancellationToken cancellationToken)
    {
        await _venueService.UnsaveAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    // ---- Admin/SuperAdmin venue management. An Admin can only act on venues assigned to them —
    // IAdminService checks that internally since [Authorize(Roles=...)] can't express it. ----

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("{id:guid}/visitors")]
    public async Task<ActionResult<PagedResult<VibeCheckDto>>> GetVisitors(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _adminService.GetVenueVisitorsAsync(User.GetUserId(), User.IsSuperAdmin(), id, page, pageSize, cancellationToken));

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:guid}/visitors/{entryId:guid}")]
    public async Task<IActionResult> RemoveVisitorVibeCheck(Guid id, Guid entryId, CancellationToken cancellationToken)
    {
        await _adminService.RemoveVisitorVibeCheckAsync(User.GetUserId(), User.IsSuperAdmin(), id, entryId, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("{id:guid}/bans")]
    public async Task<ActionResult<IReadOnlyList<BannedUserDto>>> GetBans(Guid id, CancellationToken cancellationToken)
        => Ok(await _adminService.GetBannedUsersAsync(User.GetUserId(), User.IsSuperAdmin(), id, cancellationToken));

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{id:guid}/bans")]
    public async Task<ActionResult<BannedUserDto>> BanUser(Guid id, BanUserRequest request, CancellationToken cancellationToken)
        => Ok(await _adminService.BanUserAsync(User.GetUserId(), User.IsSuperAdmin(), id, request, cancellationToken));

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:guid}/bans/{userId:guid}")]
    public async Task<IActionResult> UnbanUser(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _adminService.UnbanUserAsync(User.GetUserId(), User.IsSuperAdmin(), id, userId, cancellationToken);
        return NoContent();
    }
}
