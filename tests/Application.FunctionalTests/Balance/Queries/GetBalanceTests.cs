using N1coLoyalty.Application.Balance.Queries;
using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.Application.FunctionalTests.Balance.Queries;

using static Testing;

public class GetBalanceTests: BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    
    [Test]
    public async Task ShouldReturnCurrentBalance()
    {
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 900});
        
        var query = new GetBalanceQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.AvailableCoins.Should().Be(500);
        result.AccumulatedCoins.Should().Be(900);

    }
}
