namespace N1coLoyalty.Application.Common.Models;

public class GameSettingsDto
{
    [Obsolete("Use the `AttemptsLimit` field instead")]
    public int? UserEventLimit { get; set; }
    public int? AttemptsLimit { get; set; }
    
    [Obsolete("Use the `RemainingAttempts` field instead")]
    public int? UserEventRemaining { get; set; }
    public int? RemainingAttempts { get; set; }
    
    [Obsolete("Use the `AttemptCost` field instead")]
    public int EventCost { get; set; }
    public int AttemptCost { get; set; }
    
    [Obsolete("Use the `UnredeemedFreeAttempts` field instead")]
    public int UnredeemedFreeEvents { get; set; }
    public int UnredeemedFreeAttempts { get; set; }
    public int ExtraAttemptCost { get; set; }
}
