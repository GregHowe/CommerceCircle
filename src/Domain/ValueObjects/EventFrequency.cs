namespace N1coLoyalty.Domain.ValueObjects;

public class EventFrequency : ValueObject
{
    private readonly string _value;
    public string Value => _value;

    private EventFrequency(string value)
    {
        _value = value;
    }

    public FrequencyValue Frequency { get; private set; }

    public static EventFrequency For(string stateString)
    {
        var frequency = new EventFrequency(stateString)
        {
            Frequency = GetFrequency(stateString)
        };
        return frequency;
    }

    private static FrequencyValue GetFrequency(string frequency)
    {
        return frequency switch
        {
            "Daily" => FrequencyValue.Daily,
            "Monthly" => FrequencyValue.Monthly,
            _ => throw new InvalidOperationException($"Unknown transaction frequency: {frequency}"),
        };
    }

    public static implicit operator string(EventFrequency status)
    {
        return status.ToString();
    }

    public static explicit operator EventFrequency(string stateString)
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
