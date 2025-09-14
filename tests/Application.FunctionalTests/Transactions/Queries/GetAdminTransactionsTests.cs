using N1coLoyalty.Domain.Entities;
using N1coLoyalty.Application.UserWalletBalances.Commands.UpdateUserWalletBalance;
using N1coLoyalty.Application.Common.Constants;
using N1coLoyalty.Application.Transactions.Queries.GetAdminTransactions;
using N1coLoyalty.Application.UserWalletBalances.Commands;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.TermsConditions.Commands;
using N1coLoyalty.Domain.Enums;

namespace N1coLoyalty.Application.FunctionalTests.Transactions.Queries;

using static Testing;

public class GetAdminTransactionsTests : BaseTestFixture
{
    private Mock<IWalletsService> _walletsService = new();
    private string _userPhone;
    private readonly User _user = new() { ExternalUserId = "anyIdUser" };
    private const WalletOperationValue ActionCredit = WalletOperationValue.Credit;
    private const WalletOperationValue ActionDebit = WalletOperationValue.Debit;
    private const string Reason = "reason";
    private const int AmountCredit = 50;
    private const int AmountDebit = 80;
    private const TransactionOriginValue Origin = TransactionOriginValue.Admin;
    private const EffectSubTypeValue SubType = EffectSubTypeValue.Point;
    private const TransactionStatusValue Status = TransactionStatusValue.Redeemed;

    [SetUp]
    public async Task Setup()
    {
        // Service Mock
        _walletsService = GetServiceMock<IWalletsService>();

        // assign values 
        _userPhone = "123456";
        _user.Id = Guid.NewGuid();
        _user.Phone = _userPhone;

        // save User
        await AddAsync(_user);

        // create a new terms and conditions in the database
        var termsConditions = new TermsConditionsInfo()
        {
            Id = Guid.NewGuid(),
            Version = "2.0.0",
            Url = "https://n1co.com/terminos-y-condiciones/",
            IsCurrent = true
        };
        // save the terms and conditions in the database
        await AddAsync(termsConditions);

        // call the command to accept the terms and conditions
        var acceptTermsConditionsCommand = new AcceptTermsConditionsCommand { IsAccepted = true };
        await SendAsync(acceptTermsConditionsCommand);
    }

    [Test]
    public async Task ShouldReturnTransactionsAdmin()
    {
        // Arrange
        const int transactionsCount = 2;
        await InsertCreditOperation();
        await InsertDebitOperation();

        //Act
        var response = await SendAsync(new GetAdminTransactionsQuery());

        // Assert
        response.Should().NotBeNull();
        response.Transactions.ToList().Count.Should().Be(2);

        var transactions = response.Transactions.ToList();

        transactions.Should().HaveCount(transactionsCount);
        var transactionCredit = transactions.First(x =>
            x is { Type: EffectTypeValue.Credit, Amount: AmountCredit, Origin: Origin } && x.UserId == _user.Id && x is
            {
                SubType: SubType, Name: TransactionName.UpdateUserWalletBalance, Status: Status
            });
        transactionCredit.Should().NotBeNull();
        transactionCredit.Metadata.Should().NotBeNull();
        transactionCredit.Amount.Should().Be(AmountCredit);
        transactionCredit.Origin.Should().Be(Origin);
        transactionCredit.Status.Should().Be(Status);
        transactionCredit.SubType.Should().Be(SubType);
        transactionCredit.Description.Should().Be(Reason + ActionCredit);
        transactionCredit.Id.Should().NotBeEmpty();
        transactionCredit.Created.Month.Should().Be(DateTime.Now.Month);
        transactionCredit.RuleEffect.Should().BeNull();

        var transactionDebit = transactions.First(x =>
            x is { Type: EffectTypeValue.Debit, Amount: AmountDebit, Origin: Origin } && x.UserId == _user.Id && x is
            {
                SubType: SubType, Name: TransactionName.UpdateUserWalletBalance, Status: Status
            });
        transactionDebit.Should().NotBeNull();
        transactionDebit.Should().NotBeNull();
        transactionDebit.Metadata.Should().NotBeNull();
        transactionDebit.Amount.Should().Be(AmountDebit);
        transactionDebit.Origin.Should().Be(Origin);
        transactionDebit.Status.Should().Be(Status);
        transactionDebit.SubType.Should().Be(SubType);
        transactionDebit.Description.Should().Be(Reason + ActionDebit);
        transactionDebit.Id.Should().NotBeEmpty();
        transactionCredit.Created.Month.Should().Be(DateTime.Now.Month);
        transactionDebit.RuleEffect.Should().BeNull();
    }

    private async Task InsertCreditOperation()
    {
        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 250m, Debit = 0, TransactionId = "j181aa94-838a-418f-83e5-ed7f4bc1233c", HistoricalCredit = 500
        };
        _walletsService.Setup(x => x.Credit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = ActionCredit, Reason = Reason + ActionCredit, Amount = AmountCredit,
        };
        await SendAsync(command);
    }

    private async Task InsertDebitOperation()
    {
        var walletBalanceResponseDto = new WalletBalanceResponseDto
        {
            Credit = 0m, Debit = 250, TransactionId = "k181aa94-900a-418f-83e5-ed7f4bc4563c", HistoricalCredit = 500
        };
        _walletsService.Setup(x => x.Debit(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(walletBalanceResponseDto);

        var command = new UpdateUserWalletBalanceCommand
        {
            UserPhone = _userPhone, Operation = ActionDebit, Reason = Reason + ActionDebit, Amount = AmountDebit,
        };
        await SendAsync(command);
    }
}
