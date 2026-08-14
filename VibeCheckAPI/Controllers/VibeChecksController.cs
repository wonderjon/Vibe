using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Common;
using VibeCheck.Service.Dtos.VibeChecks;
using VibeCheck.Service.Interfaces;
using VibeCheckAPI.Extensions;

namespace VibeCheckAPI.Controllers;

[ApiController]
[Route("api/vibe-checks")]
public class VibeChecksController : ControllerBase
{
    private readonly IVibeCheckService _vibeCheckService;

    public VibeChecksController(IVibeCheckService vibeCheckService)
    {
        _vibeCheckService = vibeCheckService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<VibeCheckDto>> Create(CreateVibeCheckRequest request, CancellationToken cancellationToken)
    {
        var dto = await _vibeCheckService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VibeCheckDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _vibeCheckService.GetByIdAsync(id, User.GetUserIdOrNull(), cancellationToken));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _vibeCheckService.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    /// <summary>Personalized feed of vibe checks from users the caller follows.</summary>
    [Authorize]
    [HttpGet("feed")]
    public async Task<ActionResult<PagedResult<VibeCheckDto>>> Feed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _vibeCheckService.GetFollowingFeedAsync(User.GetUserId(), page, pageSize, cancellationToken));

    /// <summary>Global, unfiltered recent feed — works with or without auth.</summary>
    [HttpGet("feed/global")]
    public async Task<ActionResult<PagedResult<VibeCheckDto>>> GlobalFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _vibeCheckService.GetGlobalFeedAsync(User.GetUserIdOrNull(), page, pageSize, cancellationToken));

    [Authorize]
    [HttpPost("{id:guid}/react")]
    public async Task<IActionResult> React(Guid id, ReactRequest request, CancellationToken cancellationToken)
    {
        await _vibeCheckService.ReactAsync(User.GetUserId(), id, request.Type, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<PagedResult<VibeCheckCommentDto>>> GetComments(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _vibeCheckService.GetCommentsAsync(id, page, pageSize, cancellationToken));

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<VibeCheckCommentDto>> AddComment(Guid id, CreateCommentRequest request, CancellationToken cancellationToken)
        => Ok(await _vibeCheckService.AddCommentAsync(User.GetUserId(), id, request, cancellationToken));

    [Authorize]
    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        await _vibeCheckService.DeleteCommentAsync(User.GetUserId(), commentId, cancellationToken);
        return NoContent();
    }
}
