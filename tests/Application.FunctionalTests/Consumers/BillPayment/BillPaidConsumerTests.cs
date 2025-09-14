using BusinessEvents.Contracts.BillPayments;
using BusinessEvents.Contracts.BillPayments.Models;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Consumers.BillPayment;
using N1coLoyalty.Application.Users.Commands;

namespace N1coLoyalty.Application.FunctionalTests.Consumers.BillPayment;

using static Testing;
public class BillPaidConsumerTests : ConsumerBaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();

    [Test]
    public async Task ShouldConsumeEvent()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<BillPaid>(new BillPaid
        {
            Id = 1,
            Amount = 100,
            CategoryName = "anyCategory",
            ServiceName = "anyService",
            IsManagedPaymentMethod = true,
            User = new User
            {
                ExternalUserId = "anyIdUser"
            },
            DateTime = DateTime.Now,
        });

        (await IsConsumed<BillPaid, BillPaidConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Once);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailBecauseIsManagedPaymentMethodIsFalse()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0 });

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<BillPaid>(new BillPaid
        {
            Id = 1,
            Amount = 100,
            CategoryName = "anyCategory",
            ServiceName = "anyService",
            IsManagedPaymentMethod = false,
            User = new User
            {
                ExternalUserId = "anyIdUser"
            },
            DateTime = DateTime.Now,
        });

        (await IsConsumed<BillPaid, BillPaidConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
    }
}
