using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Infrastructure.Data.Repositories;

public class UserRepository(IApplicationDbContext context) : IUserRepository
{
    public async Task<User> GetOrCreateUserAsync(string externalId, string? phone)
    {
        if (externalId is null)
        {
            throw new ValidationException(new List<ValidationFailure>
            {
                new ()
                {
                    PropertyName = "externalId",
                    ErrorMessage = "externalId cannot be null."
                }
            });
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.ExternalUserId == externalId);
        
        if (user != null) return user;

        return await CreateUser(externalId, phone, CancellationToken.None);
    }

    public IQueryable<User> GetQueryableUsers()
    {
        return context.Users;
    }

    private async Task<User> CreateUser(string externalId, string? phone, CancellationToken cancellationToken)
    {
        var user = new User { ExternalUserId = externalId, Phone = phone };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
