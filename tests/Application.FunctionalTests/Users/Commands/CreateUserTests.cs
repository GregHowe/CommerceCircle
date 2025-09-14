using N1coLoyalty.Application.Common;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Users.Commands;

using static Testing;

public class CreateUserTests: BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private Mock<ILoyaltyEngine> _loyaltyEngine = new();
    
    [Test]
    public async Task ShouldCreateUser()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        //Act
        var response = await SendAsync(new CreateUserCommand());

        //Assert
        response.Success.Should().Be(true);
        response.Data!.AvailableCoins.Should().NotBeNull();
        response.Data!.AvailableCoins.Should().Be(500);
        response.Data!.AccumulatedCoins.Should().NotBeNull();
        response.Data!.AccumulatedCoins.Should().Be(500);
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.Create, WalletActionValue.Credit]);
        
        //Check transaction
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(1);
        
        var transaction = transactionList[0];
        transaction.Name.Should().Be(TransactionName.OnboardingReward);
        transaction.Amount.Should().Be(500);
        transaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transaction.TransactionOrigin.Should().Be(TransactionOriginValue.Onboarding);
    }
    
    [Test]
    public async Task ShouldNotCreateUserWhenUserAlreadyExists()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});
        
        await SendAsync(new CreateUserCommand());

        // Act
        var response = await SendAsync(new CreateUserCommand());
        
        // Assert
        response.Success.Should().Be(false);
        response.Code.Should().Be("USER_ALREADY_EXISTS");
        response.Message.Should().Be("Cuenta ya había sido creada con éxito");
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        walletBalanceList.Select(x => x.Action).Should().Contain([WalletActionValue.Create, WalletActionValue.Credit]);
        
        var onboardingTransactions = await ToListAsync<Transaction>(x => x.TransactionOrigin == TransactionOriginValue.Onboarding);
        onboardingTransactions.Should().HaveCount(1);
    }
    
    [Test]
    public async Task ShouldNotCreateUserWhenWalletCreationFails()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        
        // Act
        var response = await SendAsync(new CreateUserCommand());

        // Assert
        response.Success.Should().BeFalse();
        response.Code.Should().Be("WALLET_CREATION_ERROR");
        response.Data!.AvailableCoins.Should().NotBeNull();
        response.Data!.AvailableCoins.Should().Be(0);
        response.Data!.AccumulatedCoins.Should().NotBeNull();
        response.Data!.AccumulatedCoins.Should().Be(0);
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().BeEmpty();
    }
    
    [Test]
    public async Task ShouldNotCreateUserWhenInitialWalletCreditFails()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 , HistoricalCredit = 0});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(() => null);
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 , HistoricalCredit = 0});
        
        // Act
        var response = await SendAsync(new CreateUserCommand());

        // Assert
        response.Success.Should().BeFalse();
        response.Code.Should().Be("WALLET_CREDIT_ERROR");
        response.Data!.AvailableCoins.Should().NotBeNull();
        response.Data!.AvailableCoins.Should().Be(0);
        response.Data!.AccumulatedCoins.Should().NotBeNull();
        response.Data!.AccumulatedCoins.Should().Be(0);
        
        //Check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(1);

        var firstWalletBalance = walletBalanceList[0];
        firstWalletBalance.Action.Should().Be(WalletActionValue.Create);
        
        var transactionList = await ToListAsync<Transaction>(x => true);
        transactionList.Count.Should().Be(0);
    }

    [Test]
    public async Task Should_Fail_When_Profile_Creation_Fails()
    {
        // Arrange
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        
        _loyaltyEngine.Setup(x => x.GetProfile(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        
        _loyaltyEngine.Setup(x => x.GetOrCreateProfile(It.IsAny<string>(), It.IsAny<LoyaltyCreateProfileInput>()))
            .ReturnsAsync(new ProfileCreationDto { Success = false, Message = "Error al crear el perfil" });
        
        // Act
        var response = await SendAsync(new CreateUserCommand());
        
        // Assert
        response.Success.Should().BeFalse();
        response.Code.Should().Be("PROFILE_CREATION_ERROR");
    }
}
