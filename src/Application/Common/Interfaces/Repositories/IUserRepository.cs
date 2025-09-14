using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetOrCreateUserAsync(string externalId, string? phone);

    IQueryable<User> GetQueryableUsers();
}