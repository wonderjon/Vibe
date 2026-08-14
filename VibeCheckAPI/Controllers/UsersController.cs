using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.Users;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Interfaces;
using VibeCheckAPI.Extensions;

namespace VibeCheckAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
        => Ok(await _userService.GetMyProfileAsync(User.GetUserId(), cancellationToken));

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _userService.UpdateProfileAsync(User.GetUserId(), request, cancellationToken));

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<PublicUserDto>>> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _userService.SearchUsersAsync(q, User.GetUserIdOrNull(), page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicUserDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _userService.GetPublicProfileAsync(id, User.GetUserIdOrNull(), cancellationToken));

    [HttpGet("{id:guid}/vibe-checks")]
    public async Task<ActionResult<PagedResult<VibeCheckDto>>> GetVibeChecks(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _userService.GetUserVibeChecksAsync(id, User.GetUserIdOrNull(), page, pageSize, cancellationToken));

    [Authorize]
    [HttpPost("{id:guid}/follow")]
    public async Task<IActionResult> Follow(Guid id, CancellationToken cancellationToken)
    {
        await _userService.FollowAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid id, CancellationToken cancellationToken)
    {
        await _userService.UnfollowAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/followers")]
    public async Task<ActionResult<PagedResult<PublicUserDto>>> GetFollowers(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _userService.GetFollowersAsync(id, User.GetUserIdOrNull(), page, pageSize, cancellationToken));

    [HttpGet("{id:guid}/following")]
    public async Task<ActionResult<PagedResult<PublicUserDto>>> GetFollowing(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _userService.GetFollowingAsync(id, User.GetUserIdOrNull(), page, pageSize, cancellationToken));
}
