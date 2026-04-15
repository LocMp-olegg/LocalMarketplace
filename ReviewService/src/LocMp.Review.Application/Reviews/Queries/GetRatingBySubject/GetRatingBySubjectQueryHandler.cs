using AutoMapper;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetRatingBySubject;

public sealed class GetRatingBySubjectQueryHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<GetRatingBySubjectQuery, RatingAggregateDto>
{
    public async Task<RatingAggregateDto> Handle(GetRatingBySubjectQuery request, CancellationToken ct)
    {
        var aggregate = await db.RatingAggregates
            .FirstOrDefaultAsync(ra =>
                ra.SubjectId == request.SubjectId && ra.SubjectType == request.SubjectType, ct);

        aggregate ??= new RatingAggregate
        {
            SubjectId = request.SubjectId,
            SubjectType = request.SubjectType
        };

        return mapper.Map<RatingAggregateDto>(aggregate);
    }
}