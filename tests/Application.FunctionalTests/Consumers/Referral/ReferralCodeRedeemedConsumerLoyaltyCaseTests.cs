using BusinessEvents.Contracts.Issuing;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Consumers.Referral;
using N1coLoyalty.Application.Users.Commands;
using User = BusinessEvents.Contracts.Issuing.Models.User;

namespace N1coLoyalty.Application.FunctionalTests.Consumers.Referral;

using static Testing;

public class ReferralCodeRedeemedConsumerLoyaltyCaseTests : ConsumerBaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();

    [Test]
    public async Task ShouldConsumeEventWhenOriginIsLoyalty()
    {
        //Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                TransactionId = "mockedCreateTransactionId"
            });

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<ReferralCodeRedeemed>(new ReferralCodeRedeemed
        {
            Id = Guid.NewGuid(),
            ReferralServiceProviderId = "any",
            Origin ="loyalty",
            ReferralCode = "refCode01",
            AdvocateUser = new User
            {
                ExternalUserId = "anyIdUser"
            },
            ReferralUser = new User
            {
                ExternalUserId = "anyIdAdvocateUser"
            }
        });

        (await IsConsumed<ReferralCodeRedeemed, ReferralCodeRedeemedConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Once);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseUserNotFound()
    {
        //Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0,});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0,});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<ReferralCodeRedeemed>(new ReferralCodeRedeemed
        {
            Id = Guid.NewGuid(),
            ReferralServiceProviderId = "any",
            Origin = "loyalty",
            ReferralCode = "refCode01",
            AdvocateUser = new User
            {
                ExternalUserId = "userNotFound"
            },
            ReferralUser = new User
            {
                ExternalUserId = "anyIdAdvocateUser"
            }
        });

        (await IsConsumed<ReferralCodeRedeemed, ReferralCodeRedeemedConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
    }
}
