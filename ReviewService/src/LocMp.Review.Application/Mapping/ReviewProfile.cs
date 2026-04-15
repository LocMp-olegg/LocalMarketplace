using AutoMapper;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using ReviewEntity = LocMp.Review.Domain.Entities.Review;

namespace LocMp.Review.Application.Mapping;

public sealed class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<ReviewPhoto, ReviewPhotoDto>();
        CreateMap<ReviewResponse, ReviewResponseDto>();
        CreateMap<RatingAggregate, RatingAggregateDto>();

        CreateMap<ReviewEntity, ReviewDto>()
            .ForMember(d => d.Photos, o => o.MapFrom(s => s.Photos.OrderBy(p => p.SortOrder).ToList()))
            .ForMember(d => d.Response, o => o.MapFrom(s => s.Response));

        CreateMap<ReviewEntity, ReviewSummaryDto>()
            .ForMember(d => d.Photos, o => o.MapFrom(s => s.Photos.OrderBy(p => p.SortOrder).ToList()))
            .ForMember(d => d.Response, o => o.MapFrom(s => s.Response));
    }
}
