using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Loyalty;
using BusinessEvents.Contracts.Loyalty.Enums;
using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Consumers.Event;
using N1coLoyalty.Application.IssuingEvents;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using Challenge = BusinessEvents.Contracts.Loyalty.Models.Challenge;
using EffectStatus = BusinessEvents.Contracts.Loyalty.Enums.EffectStatus;
using Notification = Functions.Notification;

namespace N1coLoyalty.Application.FunctionalTests.Consumers.Event;

using static Testing;
public class EventProcessedConsumerTests : ConsumerBaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    
    [SetUp]
    public void ResetMock()
    {
        var notificationEventBus = GetServiceMock<INotificationEventBus>();
        notificationEventBus.Invocations.Clear();
    }
    
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
        var profileSessionId = Guid.NewGuid();
        
        var processedEvent = new BusinessEvents.Contracts.Loyalty.Models.Event()
        {
            EventType = "Expense",
            Attributes = new Dictionary<string, object>()
            {
                {"transactionId", "someTransactionId"},
                {"transactionAmount", 120}
            },
        };
        
        var effect = new Effect
        {
            Id = "POINT",
            Name = "Nivel 1 - 120 Co1ns por compra $5",
            Type = EffectType.Reward,
            Status = EffectStatus.Completed,
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
            Notification = new BusinessEvents.Contracts.Loyalty.Models.Notification
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
        await PublishMessage<EventProcessed>(new EventProcessed
        {
            ProfileSessionId = profileSessionId,
            Profile = new BusinessEvents.Contracts.Loyalty.Models.Profile
            {
                IntegrationId = "anyIdUser",
            },
            Effects = [effect],
            Event = processedEvent
        });

        (await IsConsumed<EventProcessed, EventProcessedConsumer>()).Should().Be(true);
        
        // Assert
        var transactions = await ToListAsync<Domain.Entities.Transaction>(x => true);
        transactions.Should().HaveCount(2);

        var transaction = transactions.Last();
        transaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transaction.Amount.Should().Be(120);
        
        transaction.IntegrationId.Should().NotBeNullOrEmpty();
        transaction.IntegrationId.Should().Be("someTransactionId");
        transaction.ProfileSessionId.Should().Be(profileSessionId.ToString());
        transaction.Event!.EventType.Should().Be(processedEvent.EventType);
        transaction.Event.Attributes.Should().ContainKey(processedEvent.Attributes.Keys.First());
        transaction.Event.Attributes.Should().ContainKey(processedEvent.Attributes.Keys.Last());
        transaction.Event.Attributes.Should().ContainValue("someTransactionId");
        transaction.Event.Attributes.Should().ContainValue("120");
        
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(3);

        var lastWalletBalance = walletBalanceList.Last();
        lastWalletBalance.Reason.Should().Be(transaction.Name);
        lastWalletBalance.Action.Should().Be(WalletActionValue.Credit);
        lastWalletBalance.Amount.Should().Be(120);
        lastWalletBalance.Reference.Should().NotBeNullOrEmpty();
        
        var notificationEventBusMock = GetServiceMock<INotificationEventBus>();
        notificationEventBusMock.Verify(x => x.PublishAsync(It.IsAny<Notification>()), Times.Once);
        
        var issuingEventBusMock = GetServiceMock<IIssuingEventsBus>();
        issuingEventBusMock.Verify(x => x.PublishAsync(It.IsAny<ChallengeCompleted>()), Times.Once);
    }
    
    [Test]
    public async Task ShouldConsumeTierUpgradedEvent()
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
        await PublishMessage<EventProcessed>(new EventProcessed
        {
            Profile = new BusinessEvents.Contracts.Loyalty.Models.Profile
            {
                IntegrationId = "anyIdUser",
            },
            Effects = [new Effect
            {
                Id = "TIER",
                Name = "Tier Upgraded",
                Type = EffectType.Tier,
                CampaignId = Guid.NewGuid(),
                Action = new EffectAction
                {
                    Type = EffectActionType.TierUpgrade,
                    Amount = 0m,
                    Metadata = new Dictionary<string, string>(),
                },
                Notification = new BusinessEvents.Contracts.Loyalty.Models.Notification
                {
                    Type = "TIER_UPGRADED",
                    Title = "Tier Upgraded",
                    Message = "Congratulations! You have been upgraded to new tier.",
                    FormattedMessage = "Congratulations! You have been upgraded to new tier.",
                }
            }]
        });

        (await IsConsumed<EventProcessed, EventProcessedConsumer>()).Should().Be(true);
        
        // Assert
        var transactions = await ToListAsync<Domain.Entities.Transaction>(x => true);
        transactions.Should().HaveCount(1);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseProfileIsNull()
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
        var profileSessionId = Guid.NewGuid();
        
        var processedEvent = new BusinessEvents.Contracts.Loyalty.Models.Event()
        {
            EventType = "Expense",
            Attributes = new Dictionary<string, object>()
            {
                {"transactionId", "someTransactionId"},
                {"transactionAmount", 120}
            },
        };
        
        var effect = new Effect
        {
            Id = "POINT",
            Name = "Nivel 1 - 120 Co1ns por compra $5",
            Type = EffectType.Reward,
            Status = EffectStatus.Completed,
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
            Notification = new BusinessEvents.Contracts.Loyalty.Models.Notification
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
        await PublishMessage<EventProcessed>(new EventProcessed
        {
            ProfileSessionId = profileSessionId,
            Effects = [effect],
            Event = processedEvent
        });

        (await IsConsumed<EventProcessed, EventProcessedConsumer>()).Should().Be(true);
        
        // Assert
        var transactions = await ToListAsync<Domain.Entities.Transaction>(x => true);
        transactions.Should().HaveCount(1);

        var transaction = transactions[0];
        transaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transaction.Amount.Should().Be(500);
        transaction.Name.Should().Be(TransactionName.OnboardingReward);
        
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        
        var notificationEventBusMock = GetServiceMock<INotificationEventBus>();
        notificationEventBusMock.Verify(x => x.PublishAsync(It.IsAny<Notification>()), Times.Never);
        
        var issuingEventBusMock = GetServiceMock<IIssuingEventsBus>();
        issuingEventBusMock.Verify(x => x.PublishAsync(It.IsAny<ChallengeCompleted>()), Times.Never);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseUserIsNull()
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
        var profileSessionId = Guid.NewGuid();
        
        var processedEvent = new BusinessEvents.Contracts.Loyalty.Models.Event()
        {
            EventType = "Expense",
            Attributes = new Dictionary<string, object>()
            {
                {"transactionId", "someTransactionId"},
                {"transactionAmount", 120}
            },
        };
        
        var effect = new Effect
        {
            Id = "POINT",
            Name = "Nivel 1 - 120 Co1ns por compra $5",
            Type = EffectType.Reward,
            Status = EffectStatus.Completed,
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
            Notification = new BusinessEvents.Contracts.Loyalty.Models.Notification
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
        await PublishMessage<EventProcessed>(new EventProcessed
        {
            ProfileSessionId = profileSessionId,
            Profile = new BusinessEvents.Contracts.Loyalty.Models.Profile
            {
                IntegrationId = "userNotFound",
            },
            Effects = [effect],
            Event = processedEvent
        });

        (await IsConsumed<EventProcessed, EventProcessedConsumer>()).Should().Be(true);
        
        // Assert
        var transactions = await ToListAsync<Domain.Entities.Transaction>(x => true);
        transactions.Should().HaveCount(1);

        var transaction = transactions[0];
        transaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transaction.TransactionType.Should().Be(EffectTypeValue.Reward);
        transaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transaction.Amount.Should().Be(500);
        transaction.Name.Should().Be(TransactionName.OnboardingReward);
        
        var walletBalanceList = await ToListAsync<UserWalletBalance>(x => true);
        walletBalanceList.Should().HaveCount(2);
        
        var notificationEventBusMock = GetServiceMock<INotificationEventBus>();
        notificationEventBusMock.Verify(x => x.PublishAsync(It.IsAny<Notification>()), Times.Never);
        
        var issuingEventBusMock = GetServiceMock<IIssuingEventsBus>();
        issuingEventBusMock.Verify(x => x.PublishAsync(It.IsAny<ChallengeCompleted>()), Times.Never);
    }
}
