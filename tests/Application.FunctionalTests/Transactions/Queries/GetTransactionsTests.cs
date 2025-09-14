using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Interfaces.Services.LoyaltyEngine;
using N1coLoyalty.Application.Events.Commands;
using N1coLoyalty.Application.FunctionalTests.Helpers;
using N1coLoyalty.Application.Transactions.Commands.VoidTransaction;
using N1coLoyalty.Application.Transactions.Queries.GetTransactions;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Transactions.Queries;

using static Testing;
using static TermsConditionsHelpers;

public class GetTransactionsTests : BaseTestFixture
{
    private const int TransactionsCount = 10;
    private Mock<ILoyaltyEngine> _loyaltyEngine = new();
    
    [SetUp]
    public async Task Setup()
    {
        // Arrange
        _loyaltyEngine = GetServiceMock<ILoyaltyEngine>();
        _loyaltyEngine.Setup(x => x.GetCampaign(It.IsAny<string>(),It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>()))
            .ReturnsAsync(new LoyaltyCampaignDto
            {
                Id = "id",
                Name = "Test Campaign",
                EventCost = 250,
                ConsumedBudget = 0,
                TotalBudget = 1000,
                UserEventFrequency = "Daily",
                UserEventFrequencyLimit = TransactionsCount,
                WalletConversionRate = 0.01m,
                Rewards =
                [
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "CASH",
                        Name = "Recargas al instante en tu tarjeta n1co",
                        EffectSubType = EffectSubTypeValue.Cash
                    },
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "POINT",
                        Name = "Co1ns para subir de nivel y canjear",
                        EffectSubType = EffectSubTypeValue.Point
                    },
                    new LoyaltyRewardDto
                    {
                        Id = Guid.NewGuid(),
                        IntegrationId = "COMPENSATION",
                        Name = "Gracias por participar en la ruleta",
                        EffectSubType = EffectSubTypeValue.Compensation
                    },
                    new LoyaltyRewardDto { Id = Guid.NewGuid(), IntegrationId = "RETRY", Name = "Giro de nuevo a la ruleta", }
                ]
            });

        await CreateTermsConditions();

        // update campaign event frequency limit
        const int eventAttempts = TransactionsCount;
        
        var walletsService = GetServiceMock<IWalletsService>();

        walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 5000, Debit = 0});

        walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 250, Debit = 0});

        walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0});
        
        var random = new Random();
        var randomNumbers = Enumerable.Range(0, eventAttempts).Select(_ => random.Next(0, 10)).ToList();
        
        foreach (var unused in randomNumbers)
        {
            await SendAsync(new ProcessEventCommand
            {
                EventType = EventTypeValue.PlayGame,
            });
        }
    }

    [Test]
    public async Task ShouldReturnTransactions()
    {
        // Act
        var response = await SendAsync(new GetTransactionsQuery());
        
        
        // Assert
        response.Should().NotBeNull();
        response.Transactions.Should().HaveCount(TransactionsCount * 2);
        
        var firstTransaction = response.Transactions.First();
        var transactionFromDb = await FirstAsync<Transaction>(t => t.Id == firstTransaction.Id);
        
        firstTransaction.Name.Should().Be(transactionFromDb.Name);
        firstTransaction.Description.Should().Be(transactionFromDb.Description);
        firstTransaction.Created.Should().Be(transactionFromDb.Created.AddHours(-6));
        firstTransaction.Type.Should().Be(transactionFromDb.TransactionType);
        firstTransaction.SubType.Should().Be(transactionFromDb.TransactionSubType);
        firstTransaction.Status.Should().Be(transactionFromDb.TransactionStatus);
        firstTransaction.Amount.Should().Be(transactionFromDb.Amount);
        firstTransaction.Tag.Should().Be(TransactionTag.Today);
        firstTransaction.Origin.Should().Be(transactionFromDb.TransactionOrigin);
        firstTransaction.IconCategory.Should().Be(IconCategoryValue.PointDebit);
        firstTransaction.OperationType.Should().Be(TransactionOperationTypeValue.Debit);
    }
    
    [Test]
    public async Task Should_Ignore_Transactions_When_Is_Deleted_Is_True()
    {
        var transactions = await AllToListAsync<Transaction>();
        transactions.Count.Should().Be(20);

        var transaction = transactions[0];

        transaction.IsDeleted = true;
        await UpdateAsync(transaction);
        
        var newTransactions = await AllToListAsync<Transaction>();
        newTransactions.Count.Should().Be(19);
    }
    
    [Test]
    public async Task Should_Ignore_User_Wallet_Balances_When_Is_Deleted_Is_True()
    {
        var movements = await AllToListAsync<UserWalletBalance>();
        movements.Count.Should().BeGreaterThan(1);

        foreach (var movement  in movements)
        {
            movement.IsDeleted = true;
            await UpdateAsync(movement);
        }
        
        var newTransactions = await AllToListAsync<UserWalletBalance>();
        newTransactions.Count.Should().Be(0);
    }

    [Test]
    public async Task Should_Return_Revert_Transactions_Created_By_Admin()
    {
        var transaction = await FirstAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Reward);
        
        var walletsService = GetServiceMock<IWalletsService>();
        walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0, TransactionId = Guid.NewGuid().ToString()});
        
        var command = new VoidTransactionCommand
        {
            TransactionId = transaction.Id,
            Reason = "Esta es una reversa creada desde el administrador"
        };
        
        await SendAsync(command);
        
        var response = await SendAsync(new GetTransactionsQuery());
        
        response.Should().NotBeNull();
        response.Transactions.Should().HaveCount(TransactionsCount * 2 + 1);
        
        var revertTransaction = response.Transactions.First(x=>x.Type == EffectTypeValue.Revert);
        revertTransaction.Should().NotBeNull();
        revertTransaction.Name.Should().Be(TransactionName.Revert);
        revertTransaction.Description.Should().Be(command.Reason);
        revertTransaction.OperationType.Should().Be(TransactionOperationTypeValue.CreditVoid);
        revertTransaction.Origin.Should().Be(TransactionOriginValue.Admin);
    }
    
    [Test]
    public async Task Should_Return_Refund_Transactions_Created_By_Admin()
    {
        var transaction = await FirstAsync<Transaction>(x=>x.TransactionType == EffectTypeValue.Debit);
        
        var walletsService = GetServiceMock<IWalletsService>();
        walletsService.Setup(x => x.Void(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 100, Debit = 0, TransactionId = Guid.NewGuid().ToString()});
        
        var command = new VoidTransactionCommand
        {
            TransactionId = transaction.Id,
            Reason = "Este es un reembolso creado desde el administrador"
        };
        
        await SendAsync(command);
        
        var response = await SendAsync(new GetTransactionsQuery());
        
        response.Should().NotBeNull();
        response.Transactions.Should().HaveCount(TransactionsCount * 2 + 1);
        
        var revertTransaction = response.Transactions.First(x=>x.Type == EffectTypeValue.Refund);
        revertTransaction.Should().NotBeNull();
        revertTransaction.Name.Should().Be(TransactionName.Refund);
        revertTransaction.Description.Should().Be(command.Reason);
        revertTransaction.OperationType.Should().Be(TransactionOperationTypeValue.DebitVoid);
        revertTransaction.Origin.Should().Be(TransactionOriginValue.Admin);
    }
}
