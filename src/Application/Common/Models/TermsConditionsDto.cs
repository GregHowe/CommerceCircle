namespace N1coLoyalty.Application.Common.Models;

public class TermsConditionsInfoDto
{
    public Guid Id { get; set; }
    public required string Version { get; set; }
    public required string Url { get; set; }
    public bool IsAccepted { get; set; }
}
