namespace LocMp.Identity.Api.Requests.Courier;

public sealed record UpdateCourierProfileRequest(
    bool IsActive,
    int ServiceRadiusMeters,
    double? Latitude,
    double? Longitude);