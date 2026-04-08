namespace LocMp.BuildingBlocks;

public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id), IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyCollection<IDomainEvent> Events => _events.AsReadOnly();

    public void ClearEvents() => _events.Clear();

    protected void AddEvent(IDomainEvent @event) => _events.Add(@event);
}