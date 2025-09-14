namespace N1coLoyalty.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    string ExternalId { get; }
    string? Phone { get; }
    string[] Permissions { get; }
}
