using Microsoft.AspNetCore.Mvc;
using VibeCheck.Service.Dtos.Tags;
using VibeCheck.Service.Interfaces;

namespace VibeCheckAPI.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>All selectable vibe tags, for the client's tag-picker UI.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VibeTagDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _tagService.GetAllAsync(cancellationToken));
}
