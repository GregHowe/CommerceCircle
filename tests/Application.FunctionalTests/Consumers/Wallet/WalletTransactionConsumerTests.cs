using BusinessEvents.Contracts.Loyalty.Models;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Consumers.Wallet;
using N1coLoyalty.Application.Users.Commands;
using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Consumers.Wallet;

using static Testing;
public class WalletTransactionConsumerTests : ConsumerBaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private User _user;

    [SetUp]
    public async Task SetUp()
    {
        _walletsService = GetServiceMock<IWalletsService>();
        
        _walletsService.Setup(x => x.GetBalance(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(),It.IsAny<decimal>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        _walletsService.Setup(x => x.CreateWallet(It.IsAny<string>()))
            .ReturnsAsync(new WalletBalanceResponseDto { Credit = 500, Debit = 0, HistoricalCredit = 500});
        
        await SendAsync(new CreateUserCommand());
        _user = await FirstAsync<User>(u => true);
    }
    
    
    [Test]
    public async Task Should_Not_Process_Event_When_User_Not_Found()
    {
        // Act
        await PublishMessage<WalletTransaction>(new WalletTransaction
        {
            Amount = 10m,
            Reason = "Some reason",
            Reference = "some-reference",
            Operation = WalletOperation.Debit,
            ProfileIntegrationId = "404-id", //invalid user id
            Metadata = new Dictionary<string, string>
            {
                {"AccountId", "hugoapp.h4b"}
            }
        });
        
        (await IsConsumed<WalletTransaction, WalletTransactionConsumer>()).Should().Be(true);

        var transactionCount = await CountAsync<Domain.Entities.Transaction>();
        transactionCount.Should().Be(1);
        
        var userWalletBalanceCount = await CountAsync<UserWalletBalance>();
        userWalletBalanceCount.Should().Be(2);
    }
    
    [Test]
    public async Task Should_Not_Process_Event_When_Invalid_Wallet_Operation()
    {
        // Act
        await PublishMessage<WalletTransaction>(new WalletTransaction
        {
            Amount = 10m,
            Reason = "Some reason",
            Reference = "some-reference",
            Operation = WalletOperation.Credit,
            ProfileIntegrationId = _user.ExternalUserId,
            Metadata = new Dictionary<string, string>
            {
                {"AccountId", "hugoapp.h4b"}
            }
        });
        
        (await IsConsumed<WalletTransaction, WalletTransactionConsumer>()).Should().Be(true);

        var transactionCount = await CountAsync<Domain.Entities.Transaction>();
        transactionCount.Should().Be(1);
        
        var userWalletBalanceCount = await CountAsync<UserWalletBalance>();
        userWalletBalanceCount.Should().Be(2);
    }
    
    [Test]
    public async Task Should_Not_Process_Event_When_Invalid_AccountId()
    {
        // Act
        await PublishMessage<WalletTransaction>(new WalletTransaction
        {
            Amount = 10m,
            Reason = "Some reason",
            Reference = "some-reference",
            Operation = WalletOperation.Debit,
            ProfileIntegrationId = _user.ExternalUserId,
            Metadata = new Dictionary<string, string>
            {
                {"AccountId", "invalid-account-id"}
            }
        });
        
        (await IsConsumed<WalletTransaction, WalletTransactionConsumer>()).Should().Be(true);

        var transactionCount = await CountAsync<Domain.Entities.Transaction>();
        transactionCount.Should().Be(1);
        
        var userWalletBalanceCount = await CountAsync<UserWalletBalance>();
        userWalletBalanceCount.Should().Be(2);
    }
    
    [Test]
    public async Task Should_Process_Event()
    {
        // Act
        var walletTransaction = new WalletTransaction
        {
            Amount = 10m,
            Reason = "Some reason",
            Reference = "some-reference",
            Operation = WalletOperation.Debit,
            ProfileIntegrationId = _user.ExternalUserId,
            Metadata = new Dictionary<string, string>
            {
                {"AccountId", "hugoapp.h4b"}
            }
        };
        await PublishMessage<WalletTransaction>(walletTransaction);
        
        (await IsConsumed<WalletTransaction, WalletTransactionConsumer>()).Should().Be(true);

        var transactions = await ToListAsync<Domain.Entities.Transaction>(t => t.UserId == _user.Id);
        transactions.Should().HaveCount(2);
        
        var lastTransaction = transactions.Last();
        lastTransaction.Amount.Should().Be(walletTransaction.Amount);
        lastTransaction.TransactionStatus.Should().Be(TransactionStatusValue.Redeemed);
        lastTransaction.TransactionType.Should().Be(EffectTypeValue.Debit);
        lastTransaction.TransactionSubType.Should().Be(EffectSubTypeValue.Point);
        lastTransaction.TransactionOrigin.Should().Be(TransactionOriginValue.Shop);
        
        var userWalletBalances = await ToListAsync<UserWalletBalance>(uwb => uwb.UserId == _user.Id);
        userWalletBalances.Should().HaveCount(3);
        
        var lastUserWalletBalance = userWalletBalances.Last();
        lastUserWalletBalance.Amount.Should().Be(walletTransaction.Amount);
        lastUserWalletBalance.Action.Should().Be(WalletActionValue.Debit);
        lastUserWalletBalance.Reference.Should().Be(walletTransaction.Reference);
        lastUserWalletBalance.TransactionId.Should().Be(lastTransaction.Id);
    }
}