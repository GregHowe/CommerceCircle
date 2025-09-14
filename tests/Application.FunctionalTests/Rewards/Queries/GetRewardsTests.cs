using N1coLoyalty.Application.Rewards.Queries;

namespace N1coLoyalty.Application.FunctionalTests.Rewards.Queries;

using static Testing;

public class GetRewardsTests: BaseTestFixture
{
    [Test]
    public async Task ShouldReturnOnlyAvailableRewards()
    {
        var rewards = await SendAsync(new GetAvailableRewardsQuery());
        rewards.Count.Should().Be(4);
    }
}
