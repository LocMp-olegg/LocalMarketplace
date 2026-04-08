namespace LocMp.BuildingBlocks;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}