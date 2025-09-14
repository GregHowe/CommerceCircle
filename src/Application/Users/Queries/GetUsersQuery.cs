using N1coLoyalty.Application.Common.Interfaces.Repositories;
using N1coLoyalty.Application.Common.Models;
using N1coLoyalty.Application.Common.Security;

namespace N1coLoyalty.Application.Users.Queries;

[Authorize(Permission.ReadUsers)]
public class GetUsersQuery : IRequest<UsersVm>
{
    public class GetUsersQueryHandler(
        IUserRepository userRepository) : IRequestHandler<GetUsersQuery, UsersVm>
    {
        public async Task<UsersVm> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var queryableUsers = userRepository.GetQueryableUsers();

            var queryable = queryableUsers.Select(c => new UserDto
            {
                ExternalUserId = c.ExternalUserId,
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone
            });

            return await Task.FromResult(new UsersVm
            {
                Users = queryable
            });
        }
    }
}