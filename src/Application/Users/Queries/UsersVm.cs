using N1coLoyalty.Application.Common.Models;

namespace N1coLoyalty.Application.Users.Queries;

public class UsersVm
{
    public required IQueryable<UserDto> Users { get; set; }
}