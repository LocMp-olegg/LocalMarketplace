using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Tags.GetAllTags;

public sealed record GetAllTagsQuery : IRequest<IReadOnlyList<TagDto>>, ICacheableQuery
{
    public string CacheKey => "tags:all";
    public TimeSpan Ttl => TimeSpan.FromHours(1);
}
