namespace N1coLoyalty.Domain.Common;

// Learn more: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
public abstract class ValueObject
{
    private static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }

        return left?.Equals(right) != false;
    }

    private static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        using var thisValues = GetAtomicValues().GetEnumerator();
        using var otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (thisValues.Current is null ^ otherValues.Current is null)
            {
                return false;
            }

            if (thisValues.Current != null &&
                !thisValues.Current.Equals(otherValues.Current))
            {
                return false;
            }
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    public static bool operator ==(ValueObject left, ValueObject right) => EqualOperator(left, right);
    public static bool operator !=(ValueObject left, ValueObject right) => NotEqualOperator(left, right);

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
