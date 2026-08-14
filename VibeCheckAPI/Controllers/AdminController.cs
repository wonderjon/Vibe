using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Admin;
using VibeCheck.Service.Interfaces;

namespace VibeCheckAPI.Controllers;

/// <summary>
/// SuperAdmin-only: create/list/delete Admin accounts and manage which venues each Admin is
/// assigned to. There is deliberately no self-registration path to an Admin or SuperAdmin role —
/// Admins only ever come from here, and SuperAdmin only ever comes from startup seeding.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("admins")]
    public async Task<ActionResult<AdminDto>> CreateAdmin(CreateAdminRequest request, CancellationToken cancellationToken)
        => Ok(await _adminService.CreateAdminAsync(request, cancellationToken));

    [HttpGet("admins")]
    public async Task<ActionResult<IReadOnlyList<AdminDto>>> GetAdmins(CancellationToken cancellationToken)
        => Ok(await _adminService.GetAdminsAsync(cancellationToken));

    [HttpDelete("admins/{id:guid}")]
    public async Task<IActionResult> DeleteAdmin(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteAdminAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("admins/{id:guid}/venues")]
    public async Task<IActionResult> AssignVenue(Guid id, AssignVenueRequest request, CancellationToken cancellationToken)
    {
        await _adminService.AssignVenueAsync(id, request.VenueId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("admins/{id:guid}/venues/{venueId:guid}")]
    public async Task<IActionResult> UnassignVenue(Guid id, Guid venueId, CancellationToken cancellationToken)
    {
        await _adminService.UnassignVenueAsync(id, venueId, cancellationToken);
        return NoContent();
    }
}
