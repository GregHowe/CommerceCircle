namespace N1coLoyalty.Domain.ValueObjects;

public class BenefitType: ValueObject
{
    private readonly string _value;
    public string Value => _value;

    private BenefitType(string value)
    {
        _value = value;
    }

    public BenefitTypeValue Type { get; private set; }

    public static BenefitType For(string stateString)
    {
        var subType = new BenefitType(stateString)
        {
            Type = GetType(stateString)
        };

        return subType;
    }

    private static BenefitTypeValue GetType(string type)
    {
        return type switch
        {
            "ShopAccess" => BenefitTypeValue.ShopAccess,
            "ConcertDiscounts" => BenefitTypeValue.ConcertDiscounts,
            "Rewards" => BenefitTypeValue.Rewards,
            "UNKNOWN" => BenefitTypeValue.Unknown,
            _ => throw new InvalidOperationException($"Unknown benefit type: {type}"),
        };
    }

    public static implicit operator string(BenefitType status)
    {
        return status.ToString();
    }

    public static explicit operator BenefitType(string stateString)
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
