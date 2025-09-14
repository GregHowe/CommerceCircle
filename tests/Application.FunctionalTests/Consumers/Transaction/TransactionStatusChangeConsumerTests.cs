using BusinessEvents.Contracts.Issuing;
using BusinessEvents.Contracts.Issuing.Models;
using BusinessEvents.Contracts.Loyalty;
using BusinessEvents.Contracts.Loyalty.Enums;
using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Consumers.Event;
using N1coLoyalty.Application.Consumers.Transaction;
using N1coLoyalty.Application.Consumers.Transaction.Common;
using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Application.NotificationEvents;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;
using Challenge = BusinessEvents.Contracts.Loyalty.Models.Challenge;
using ContractUser = BusinessEvents.Contracts.Issuing.Models.User;
using Effect = BusinessEvents.Contracts.Loyalty.Models.Effect;
using EffectStatus = BusinessEvents.Contracts.Loyalty.Enums.EffectStatus;
using User = N1coLoyalty.Domain.Entities.User;


namespace N1coLoyalty.Application.FunctionalTests.Consumers.Transaction;

using static Testing;
using static TransactionHelpers;
using static UserWalletBalanceHelpers;

public class TransactionStatusChangeConsumerTests : ConsumerBaseTestFixture
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
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
        {
            Id = Guid.NewGuid(),
            Amount = 101m,
            TransactionType = TransactionTypeConstant.Expense,
            TransactionStatus = TransactionStatusConstant.Completed,
            ApprovalStatus = "APPROVED",
            OperationType = "DEBIT",
            User = new ContractUser
            {
                ExternalUserId = "anyIdUser"
            }
        });

        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Once);
    }
    
    [Test]
    public async Task ShouldConsumeEventWithPosMetadata()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
        {
            Id = Guid.NewGuid(),
            Amount = 101m,
            TransactionType = TransactionTypeConstant.Expense,
            TransactionStatus = TransactionStatusConstant.Completed,
            ApprovalStatus = "APPROVED",
            OperationType = "DEBIT",
            User = new ContractUser
            {
                ExternalUserId = "anyIdUser"
            },
            PosMetadata = new PosMetadata()
            {
                TerminalId = "AnyTerminalId",
                MerchantId = "AnyMerchantId",
                Mcc = "anyMccId",
            }
        });

        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Once);
    }

    [Test]
    public async Task ShouldConsumeRevertedEvent()
    {
        #region Consume Expense Event

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
                {"transactionId", Guid.NewGuid()},
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
            Notification = new Notification
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

        await IsConsumed<EventProcessed, EventProcessedConsumer>();

        #endregion

        var challengeCompletedTransaction = await FirstAsync<Domain.Entities.Transaction>(x=>x.TransactionOrigin == TransactionOriginValue.Challenge);
        
        challengeCompletedTransaction.Should().NotBeNull();
        
        // Act
        if (challengeCompletedTransaction.IntegrationId != null)
        {
            await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
            {
                Id = new Guid(challengeCompletedTransaction.IntegrationId),
                Amount = 120m,
                TransactionType = TransactionTypeConstant.Expense,
                TransactionStatus = TransactionStatusConstant.Reverted,
                ApprovalStatus = "APPROVED",
                OperationType = "DEBIT",
                User = new ContractUser { ExternalUserId = "anyIdUser" }
            });
        }
        
        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
        
        loyaltyEngineMock.Verify(x => x.VoidSessionAsync(It.IsAny<VoidSessionInputDto>()), Times.Once);
    }
    
    [Test]
    public async Task ShouldConsumeRevertedEventWithoutEffects()
    {
        // Act
        
            await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
            {
                Id = Guid.NewGuid(),
                Amount = 120m,
                TransactionType = TransactionTypeConstant.Expense,
                TransactionStatus = TransactionStatusConstant.Reverted,
                ApprovalStatus = "APPROVED",
                OperationType = "DEBIT",
                User = new ContractUser { ExternalUserId = "anyIdUser" }
            });
        
            (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
            var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
            loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
        
            loyaltyEngineMock.Verify(x => x.VoidSession(It.IsAny<VoidSessionInputDto>()), Times.Never);
    }
    
    [Test]
    public async Task ShouldConsumeRevertedEventWithoutProfileSessionId()
    {
        await SendAsync(new CreateUserCommand());
        var user = await FirstAsync<User>(x => true);
        var transaction = await CreateTransactionMock(user, 500);
        await CreateUserWalletBalanceMock(transaction, user, WalletActionValue.Credit);
        
        transaction.Should().NotBeNull();
        
        // Configure Revert
        _walletsService = GetServiceMock<IWalletsService>();
        _walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 0 });
        
        // Act
        if (transaction.IntegrationId != null)
        {
            await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
            {
                Id = new Guid(transaction.IntegrationId),
                Amount = 500m,
                TransactionType = TransactionTypeConstant.Expense,
                TransactionStatus = TransactionStatusConstant.Reverted,
                ApprovalStatus = "APPROVED",
                OperationType = "DEBIT",
                User = new ContractUser { ExternalUserId = "anyIdUser" }
            });
        }
        
        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
        loyaltyEngineMock.Verify(x => x.VoidSession(It.IsAny<VoidSessionInputDto>()), Times.Never);
        
        var transactionVoid = await FirstAsync<Domain.Entities.Transaction>(x => x.TransactionType == EffectTypeValue.Revert);
        transactionVoid.Should().NotBeNull();
        transactionVoid.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        transactionVoid.Name.Should().Be(TransactionName.Revert);
        transactionVoid.UserId.Should().Be(user.Id);
        transactionVoid.Description.Should().Be(TransactionDescription.Challenge);
        transactionVoid.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        transactionVoid.TransactionOrigin.Should().Be(TransactionOriginValue.Challenge);
        transactionVoid.Amount.Should().Be(500);

        var userWalletBalance = await FirstAsync<UserWalletBalance>(x => x.Action == WalletActionValue.CreditVoid);
        userWalletBalance.Should().NotBeNull();
        userWalletBalance.Reason.Should().Be(TransactionName.Revert);
        userWalletBalance.UserId.Should().Be(user.Id);
        userWalletBalance.TransactionId.Should().Be(transactionVoid.Id);
        userWalletBalance.Amount.Should().Be(500);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseInvalidTransactionType()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
        {
            Id = Guid.NewGuid(),
            Amount = 101m,
            TransactionType = "INVALID_TRANSACTION_TYPE",
            TransactionStatus = TransactionStatusConstant.Completed,
            ApprovalStatus = "APPROVED",
            OperationType = "DEBIT",
            User = new ContractUser
            {
                ExternalUserId = "anyIdUser"
            }
        });

        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseInvalidTransactionStatus()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
        {
            Id = Guid.NewGuid(),
            Amount = 101m,
            TransactionType = TransactionTypeConstant.Expense,
            TransactionStatus = "INVALID_TRANSACTION_STATUS",
            ApprovalStatus = "APPROVED",
            OperationType = "DEBIT",
            User = new ContractUser
            {
                ExternalUserId = "anyIdUser"
            }
        });

        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
    }
    
    [Test]
    public async Task ShouldConsumeEventButFailedBecauseTransactionToRevertNotFound()
    {
        // Arrange
        _walletsService = GetServiceMock<IWalletsService>();

        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto
            {
                Credit = 500,
                Debit = 0,
                TransactionId = "mockedTransactionId",
            });

        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0});

        await SendAsync(new CreateUserCommand());

        // Act
        await PublishMessage<TransactionStatusChange>(new TransactionStatusChange
        {
            Id = Guid.NewGuid(),
            Amount = 101m,
            TransactionType = TransactionTypeConstant.Expense,
            TransactionStatus = TransactionStatusConstant.Reverted,
            ApprovalStatus = "APPROVED",
            OperationType = "DEBIT",
            User = new ContractUser
            {
                ExternalUserId = "anyIdUser"
            }
        });

        (await IsConsumed<TransactionStatusChange, TransactionStatusChangeConsumer>()).Should().Be(true);
        
        var loyaltyEngineMock = GetServiceMock<ILoyaltyEngine>();
        loyaltyEngineMock.Verify(x => x.ProcessEventAsync(It.IsAny<ProcessEventInputDto>()), Times.Never);
    }
}
