namespace LocMp.Contracts;

public interface IIntegrationEvent
{
    DateTimeOffset OccurredAt { get; init; }
}