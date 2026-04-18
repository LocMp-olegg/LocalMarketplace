namespace LocMp.Catalog.Api.Requests.Shops;

public sealed record SetCourierDeliveryRequest(bool Allow, int? MaxDistanceMeters);
