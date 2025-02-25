namespace Domain.ValueObjects;

public abstract class ValueObject<T> : IEquatable<T> where T : ValueObject<T>
{
    public abstract bool Equals(T other);

    public static bool operator ==(ValueObject<T> a, ValueObject<T> b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject<T> a, ValueObject<T> b)
    {
        return !(a == b);
    }
}
