using VibeCheck.Domain.Entities;
using VibeCheck.Service.Dtos.Tags;

namespace VibeCheck.Service.Mapping;

public static class TagMappingExtensions
{
    public static VibeTagDto ToDto(this VibeTag tag) => new(tag.Id, tag.Name);
}
