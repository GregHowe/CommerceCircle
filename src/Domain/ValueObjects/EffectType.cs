namespace N1coLoyalty.Domain.ValueObjects;

public class EffectType: ValueObject
{
    private readonly string _value;
    public string Value => _value;

    private EffectType(string value)
    {
        _value = value;
    }

    public EffectTypeValue Type { get; private set; }

    public static EffectType For(string stateString)
    {
        var type = new EffectType(stateString)
        {
            Type = GetType(stateString)
        };

        return type;
    }

    private static EffectTypeValue GetType(string type)
    {
        type = type.ToUpperInvariant();
        return type switch
        {
            "REWARD" => EffectTypeValue.Reward,
            _ => throw new InvalidOperationException($"Unknown effect type: {type}"),
        };
    }

    public static implicit operator string(EffectType status)
    {
        return status.ToString();
    }

    public static explicit operator EffectType(string stateString)
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
