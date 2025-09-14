using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace N1coLoyalty.Infrastructure.Identity;

public class IdentityService(IApplicationDbContext context) : IIdentityService
{
    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await context.Users.FindAsync(userId);

        return user?.Name;
    }

    public async Task<string?> GetUserNameByExternalIdAsync(string externalUserId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId);

        return user?.Name;
    }

    

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(string userName, string? phone, string externalUserId)
    {
        var user = new User
        {
            Name = userName,
            Phone = phone,
            ExternalUserId = externalUserId,
        };

        var entity = await context.Users.AddAsync(user);

        return (Result.Success(), entity.Entity.Id);
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        return Task.FromResult(true);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await context.Users.FindAsync();

        if (user == null)
        {
            return false;
        }
        
        return true;
    }
}
