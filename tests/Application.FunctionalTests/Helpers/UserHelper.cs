using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application.FunctionalTests.Helpers;

using static Testing;
internal static class UserHelper
{
    internal static async Task<IList<User>?> CreateUsersMock()
    {
        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "user1",
                Phone = "0000000000",
                ExternalUserId = "anyExternalUserId1"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "user2",
                Phone = "222222222",
                ExternalUserId = "anyExternalUserId2"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "user3",
                Phone = "333333333",
                ExternalUserId = "anyExternalUserId3"
            }
        };

        foreach (var user in users)
        {
            await AddAsync(user);
        }

        return await ToListAsync<User>(t => true);
    }
}