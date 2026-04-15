using AutoMapper;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewById;

public sealed class GetReviewByIdQueryHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
{
    public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken ct)
    {
        var review = await db.Reviews
            .Include(r => r.Photos)
            .Include(r => r.Response)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct);

        return review is null ? null : mapper.Map<ReviewDto>(review);
    }
}
