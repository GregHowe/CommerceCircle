using System.Security.Claims;
using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.AdminApi.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    private const string EmailClaim = "https://h4b.dev/claims/email";
    private const string PermissionsClaim = "permissions";

    public string? Id => _user?.FindFirstValue(EmailClaim) ?? _user?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string ExternalId => Id ?? string.Empty;
    public string? Phone => null;

    public string[] Permissions => _user?.FindAll(PermissionsClaim).Select(r => r.Value).ToArray() ?? [];
}