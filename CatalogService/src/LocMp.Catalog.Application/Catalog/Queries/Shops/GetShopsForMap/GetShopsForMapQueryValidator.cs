using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsForMap;

public sealed class GetShopsForMapQueryValidator : AbstractValidator<GetShopsForMapQuery>
{
    public GetShopsForMapQueryValidator()
    {
        RuleFor(x => x.SwLat).InclusiveBetween(-90, 90);
        RuleFor(x => x.NeLat).InclusiveBetween(-90, 90);
        RuleFor(x => x.SwLon).InclusiveBetween(-180, 180);
        RuleFor(x => x.NeLon).InclusiveBetween(-180, 180);

        RuleFor(x => x)
            .Must(x => x.SwLat < x.NeLat)
            .WithMessage("swLat must be less than neLat.")
            .Must(x => x.SwLon < x.NeLon)
            .WithMessage("swLon must be less than neLon.");
    }
}