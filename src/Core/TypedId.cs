namespace Core;

public abstract class TypedId(Guid value) : IEquatable<TypedId>
{
    public Guid Value { get; } = value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is TypedId other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(TypedId? other) => Value == other?.Value;

    public static bool operator ==(TypedId? obj1, TypedId? obj2)
    {
        return obj1?.Equals(obj2) ?? Equals(obj2, null);
    }

    public static bool operator !=(TypedId x, TypedId y) => !(x == y);
}