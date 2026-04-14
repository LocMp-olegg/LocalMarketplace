using AutoMapper;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewByOrder;

public sealed class GetReviewByOrderQueryHandler(ReviewDbContext db, IMapper mapper)
    : IRequestHandler<GetReviewByOrderQuery, ReviewDto?>
{
    public async Task<ReviewDto?> Handle(GetReviewByOrderQuery request, CancellationToken ct)
    {
        var review = await db.Reviews
            .Include(r => r.Photos)
            .Include(r => r.Response)
            .FirstOrDefaultAsync(r => r.OrderId == request.OrderId, ct);

        return review is null ? null : mapper.Map<ReviewDto>(review);
    }
}