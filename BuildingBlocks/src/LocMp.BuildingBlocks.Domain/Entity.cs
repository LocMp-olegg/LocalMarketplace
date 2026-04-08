namespace LocMp.BuildingBlocks;

public abstract class Entity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; init; } = id;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other &&
        GetType() == other.GetType() &&
        Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}