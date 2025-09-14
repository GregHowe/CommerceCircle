using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Loyalty;
using BusinessEvents.Contracts.Loyalty.Enums;
using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Consumers.Event;
using N1coLoyalty.Application.Consumers.Session;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Application.Transactions.Queries.GetTransactions;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using Challenge = BusinessEvents.Contracts.Loyalty.Models.Challenge;
using EffectStatus = BusinessEvents.Contracts.Loyalty.Enums.EffectStatus;
using Notification = Functions.Notification;
using NotificationContract = BusinessEvents.Contracts.Loyalty.Models.Notification;

namespace N1coLoyalty.Application.FunctionalTests.Consumers.Session;

using static Testing;
public class ProfileSessionEffectsVoidedConsumerTests : ConsumerBaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();

    [Test]
    public async Task ShouldProcessEvent()
    {
        // Arrange
        
        // Mocking WalletsService
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

        // Creating a user
        await SendAsync(new CreateUserCommand());
        
        // Processing a completed event
        const int expectedRewardAmount = 120;
        var @event = new BusinessEvents.Contracts.Loyalty.Models.Event
        {
            EventType = "Expense",
            Attributes = new Dictionary<string, object>()
            {
                {"transactionId", "someTransactionId"},
                {"transactionAmount", expectedRewardAmount}
            },
        };
        
        var originalProfileSessionId = Guid.NewGuid();
        var completedEffect = GetEffect();
        await PublishMessage<EventProcessed>(new EventProcessed
        {
            ProfileSessionId = originalProfileSessionId,
            Event = @event,
            Effects = [completedEffect],
            Status = ProfileSessionStatus.Closed,
            Profile = new BusinessEvents.Contracts.Loyalty.Models.Profile
            {
                IntegrationId = "anyIdUser",
                PhoneNumber = "123456789",
            }
        });
        (await IsConsumed<EventProcessed, EventProcessedConsumer>()).Should().Be(true);
        
        var voidProfileSessionId = Guid.NewGuid();
        var voidedEffect = GetEffect(EffectStatus.Voided);
        
        // check notification event
        var notificationEventBusMock = GetServiceMock<INotificationEventBus>();
        notificationEventBusMock.Verify(x => x.PublishAsync(It.IsAny<Notification>()), Times.Once);
        notificationEventBusMock.Invocations.Clear();
        
        // check issuing event
        var issuingEventBusMock = GetServiceMock<IIssuingEventsBus>();
        issuingEventBusMock.Verify(x => x.PublishAsync(It.IsAny<ChallengeCompleted>()), Times.Once);
        issuingEventBusMock.Invocations.Clear();
        
        // Act
        await PublishMessage<ProfileSessionEffectsVoided>(new ProfileSessionEffectsVoided
        {
            OriginalProfileSession = new ProfileSession
            {
                ProfileSessionId = originalProfileSessionId,
            },
            VoidProfileSession = new ProfileSession
            {
                ProfileSessionId = voidProfileSessionId,
                Event = @event,
                Effects = [voidedEffect],
                Status = ProfileSessionStatus.Closed,
                Profile = new BusinessEvents.Contracts.Loyalty.Models.Profile
                {
                    IntegrationId = "anyIdUser",
                    PhoneNumber = "123456789",
                }
            }
        });
        
        (await IsConsumed<ProfileSessionEffectsVoided, ProfileSessionEffectsVoidedConsumer>()).Should().Be(true);
        
        // Assert
        var transactions = await ToListAsync<Domain.Entities.Transaction>(x => true);
        transactions.Should().HaveCount(3);
        
        // check original transaction
        var originalTransaction = transactions.First(t => t.TransactionStatus == TransactionStatusValue.Voided);
        originalTransaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        originalTransaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        originalTransaction.Amount.Should().Be(expectedRewardAmount);
        originalTransaction.ProfileSessionId.Should().NotBeNullOrEmpty();
        originalTransaction.ProfileSessionId.Should().Be(originalProfileSessionId.ToString());
        originalTransaction.Metadata.Should().ContainKey(TransactionMetadata.VoidProfileSessionId);
        originalTransaction.Metadata.Should().ContainValue(voidProfileSessionId.ToString());
        
        // check void transaction
        var voidTransaction = transactions.First(t => t.TransactionType == EffectTypeValue.Revert);
        voidTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        voidTransaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        voidTransaction.Amount.Should().Be(expectedRewardAmount);
        voidTransaction.ProfileSessionId.Should().NotBeNullOrEmpty();
        voidTransaction.ProfileSessionId.Should().Be(voidProfileSessionId.ToString());
        voidTransaction.Metadata.Should().ContainKey(TransactionMetadata.VoidedProfileSessionId);
        voidTransaction.Metadata.Should().ContainValue(originalProfileSessionId.ToString());
        
        // check wallet balance
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(4);
        var lastWalletBalance = walletBalanceList.Last();
        lastWalletBalance.Reason.Should().Be(TransactionName.Revert);
        lastWalletBalance.Action.Should().Be(WalletActionValue.CreditVoid);
        lastWalletBalance.Amount.Should().Be(expectedRewardAmount);
        lastWalletBalance.Reference.Should().NotBeNullOrEmpty();
        lastWalletBalance.TransactionId.Should().Be(voidTransaction.Id);
        
        // check notification event
        notificationEventBusMock.Verify(x => x.PublishAsync(It.IsAny<Notification>()), Times.Never);
        
        // check issuing event
        issuingEventBusMock.Verify(x => x.PublishAsync(It.IsAny<ChallengeCompleted>()), Times.Never);
        
        var response = await SendAsync(new GetTransactionsQuery());
        
        response.Should().NotBeNull();
        response.Transactions.Should().HaveCount(3);
        
        var revertTransaction = response.Transactions.First(x=>x.Type == EffectTypeValue.Revert);
        revertTransaction.Should().NotBeNull();
        revertTransaction.Name.Should().Be(TransactionName.Revert);
        revertTransaction.Description.Should().Be(TransactionDescription.Challenge);
        revertTransaction.OperationType.Should().Be(TransactionOperationTypeValue.CreditVoid);
        revertTransaction.Origin.Should().Be(TransactionOriginValue.Challenge);
    }

    private static Effect GetEffect(EffectStatus status = EffectStatus.Completed)
    {
        return new Effect
        {
            Id = "POINT",
            Name = "Nivel 1 - 120 Co1ns por compra $5",
            Type = EffectType.Reward,
            Status = status,
            CampaignId = Guid.NewGuid(),
            Action = new EffectAction
            {
                Type = EffectActionType.AddPoints,
                Amount = 120,
                Metadata = new Dictionary<string, string>
                {
                    {"transactionId", "someTransactionId"}
                },
            },
            Notification = new NotificationContract
            {
                Type = "EXPENSE_CHALLENGE_COMPLETED",
                Title = "Compra con n1co",
                Message = "Tu desafío ha sido cumplido, has acumulado 120 co1ns.",
                FormattedMessage = "<b>Has comprado con n1co</b> y completado un reto en co1ns. ¡Sigue ganando!",
            },
            Challenge = new Challenge
            {
                Id = Guid.NewGuid(),
                Name = "Compra con tarjeta n1co",
                Description = "120 Co1ns por compra $5",
                Target = 1,
                Metadata = new Dictionary<string, string>
                {
                    {"type", "Expense"},
                    {"effectValue", "200"},
                    {"effectType", "Reward"},
                    {"effectSubType", "Point"},
                }
            }
        };
    }
}
