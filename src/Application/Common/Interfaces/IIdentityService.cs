using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<string?> GetUserNameByExternalIdAsync(string userId);
    
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, Guid UserId)> CreateUserAsync(string userName, string? phone, string externalUserId);
}
