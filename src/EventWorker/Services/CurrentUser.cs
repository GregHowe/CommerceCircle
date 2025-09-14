using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.EventWorker.Services;

public class CurrentUser : IUser
{
    public string? Id => "system";
    public string ExternalId => "system";
    public string? Phone => "";
    public string[] Permissions => [];
}