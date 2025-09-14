namespace N1coLoyalty.Domain.ValueObjects;

public class TransactionSubType: ValueObject
{
    private readonly string _value;
    public string Value => _value;

    private TransactionSubType(string value)
    {
        _value = value;
    }

    public EffectSubTypeValue SubType { get; private set; }

    public static TransactionSubType For(string stateString)
    {
        var subType = new TransactionSubType(stateString)
        {
            SubType = GetSubType(stateString)
        };

        return subType;
    }

    private static EffectSubTypeValue GetSubType(string subType)
    {
        return subType switch
        {
            "CASH" => EffectSubTypeValue.Cash,
            "POINT" => EffectSubTypeValue.Point,
            "COMPENSATION" => EffectSubTypeValue.Compensation,
            "RETRY" => EffectSubTypeValue.Retry,
            "UNKNOWN" => EffectSubTypeValue.Unknown,
            _ => throw new InvalidOperationException($"Unknown transaction sub type: {subType}"),
        };
    }

    public static implicit operator string(TransactionSubType status)
    {
        return status.ToString();
    }

    public static explicit operator TransactionSubType(string stateString)
    {
        return For(stateString);
    }

    public override string ToString()
    {
        return _value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return _value;
    }
}
