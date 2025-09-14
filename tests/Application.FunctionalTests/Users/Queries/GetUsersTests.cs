using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Application.Users.Queries;

namespace N1coLoyalty.Application.FunctionalTests.Users.Queries;

using static Testing;
using static UserHelper;

public class GetUsersTests : BaseTestFixture
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public async Task Should_Return_All_Users()
    {
        //arrange
        var users = await CreateUsersMock();

        var firstUser = users?.First();
        var lastUser = users?.Last();

        var query = new GetUsersQuery();

        //action

        var result = await SendAsync(query);

        //assert

        result.Should().NotBeNull();
        result.Users.Should().HaveCount(3);

        var userDtos = result.Users.ToList();
        var firstResult = userDtos.First();
        firstResult.Should().NotBeNull();
        firstResult.Id.Should().Be(firstUser!.Id);
        firstResult.Name.Should().Be(firstUser.Name);
        firstResult.ExternalUserId.Should().Be(firstUser.ExternalUserId);
        firstResult.Phone.Should().Be(firstUser.Phone);

        var lastResult = userDtos.Last();
        lastResult.Should().NotBeNull();
        lastResult.Id.Should().Be(lastUser!.Id);
        lastResult.Name.Should().Be(lastUser.Name);
        lastResult.ExternalUserId.Should().Be(lastUser.ExternalUserId);
        lastResult.Phone.Should().Be(lastUser.Phone);
    }
}