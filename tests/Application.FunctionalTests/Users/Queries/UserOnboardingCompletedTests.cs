using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Profile.Queries;
using N1coLoyalty.Application.Users.Queries;

namespace N1coLoyalty.Application.FunctionalTests.Users.Queries;
using static Testing;

public class UserOnboardingCompletedTests: BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    
    [Test]
    public async Task Profile_Exists_Returns_True()
    {
        #region Signup & Onboarding
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        //Act
        await SendAsync(new GetProfileQuery());
        #endregion
        
        var response = await SendAsync(new UserOnboardingCompletedQuery());
        response.Should().NotBeNull();
        response.Completed.Should().BeTrue();
    }
    
    [Test]
    public async Task Profile_Exists_Returns_False_Because_User_Is_Null()
    {
        var response = await SendAsync(new UserOnboardingCompletedQuery());
        response.Should().NotBeNull();
        response.Completed.Should().BeFalse();
    }
    
    [Test]
    public async Task Profile_Exists_Returns_False_Because_Credit_Failed()
    {
        #region Signup & Onboarding
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(()=> null);
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        //Act
        await SendAsync(new GetProfileQuery());
        #endregion
        
        var response = await SendAsync(new UserOnboardingCompletedQuery());
        response.Should().NotBeNull();
        response.Completed.Should().BeFalse();
    }
    
    [Test]
    public async Task Profile_Exists_Returns_False_Because_Create_Wallet_Failed()
    {
        #region Signup & Onboarding
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 0, Debit = 0, HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        
        //Act
        await SendAsync(new GetProfileQuery());
        #endregion
        
        var response = await SendAsync(new UserOnboardingCompletedQuery());
        response.Should().NotBeNull();
        response.Completed.Should().BeFalse();
    }
}
