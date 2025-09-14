using System.Security.Claims;
using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.Api.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string ExternalId =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue("https://n1/claims/user/id") ?? string.Empty;

    public string? Phone => httpContextAccessor.HttpContext?.User?.FindFirstValue("https://n1/claims/phone");
    public string[] Permissions => [];
}
