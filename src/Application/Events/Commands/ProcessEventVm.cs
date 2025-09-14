namespace N1coLoyalty.Application.Events.Commands;

public class ProcessEventVm
{
    public decimal EventCost { get; set; }
    public required EffectBalanceDto Balance { get; set; }
    public EffectDto? Effect { get; set; }
}
